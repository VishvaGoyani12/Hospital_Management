using Appointment_Management_Blazor.Data;
using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Appointment_Management_Blazor.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DoctorService(ApplicationDbContext context,
                             UserManager<ApplicationUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<object> GetAllDoctorsAsync(DoctorFilterModel filters)
        {
            int pageSize = filters.Length != 0 ? filters.Length : 10;
            int skip = filters.Start;

            var query = _context.Doctors.Include(d => d.ApplicationUser).AsQueryable();

            if (!string.IsNullOrEmpty(filters.Gender))
                query = query.Where(d => d.ApplicationUser.Gender == filters.Gender);

            if (!string.IsNullOrEmpty(filters.Status) && bool.TryParse(filters.Status, out var boolStatus))
                query = query.Where(d => d.Status == boolStatus);

            if (!string.IsNullOrEmpty(filters.SpecialistIn))
                query = query.Where(d => d.SpecialistIn == filters.SpecialistIn);

            if (!string.IsNullOrEmpty(filters.SearchValue))
            {
                query = query.Where(d =>
                    d.ApplicationUser.FullName.Contains(filters.SearchValue) ||
                    d.ApplicationUser.Gender.Contains(filters.SearchValue) ||
                    d.SpecialistIn.Contains(filters.SearchValue));
            }

            var total = query.Count();

            var sortMap = new Dictionary<string, string>
            {
                ["fullName"] = "ApplicationUser.FullName",
                ["gender"] = "ApplicationUser.Gender",
                ["specialistIn"] = "SpecialistIn",
                ["status"] = "Status"
            };

            if (!string.IsNullOrEmpty(filters.SortColumn) && sortMap.TryGetValue(filters.SortColumn, out var mappedColumn))
            {
                query = query.OrderBy($"{mappedColumn} {filters.SortDirection}");
            }

            var data = await query.Skip(skip).Take(pageSize).Select(d => new
            {
                id = d.ApplicationUserId,
                fullName = d.ApplicationUser.FullName,
                gender = d.ApplicationUser.Gender,
                specialistIn = d.SpecialistIn,
                status = d.Status ? "Active" : "Deactive"
            }).ToListAsync();

            return new
            {
                draw = filters.Draw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            };
        }


        public async Task<(bool Success, string Message)> CreateDoctorAsync(DoctorViewModel model)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return (false, "A doctor with this email already exists.");

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Gender = model.Gender,
                Email = model.Email,
                UserName = model.Email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, string.Join("<br/>", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("Doctor"))
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                ApplicationUserId = user.Id,
                SpecialistIn = model.SpecialistIn,
                Status = model.Status
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return (true, "Doctor created successfully");
        }

        public async Task<DoctorViewModel?> GetDoctorByIdAsync(string id)
        {
            var doctor = await _context.Doctors.Include(d => d.ApplicationUser)
                                               .FirstOrDefaultAsync(d => d.ApplicationUserId == id);
            if (doctor == null)
                return null;

            return new DoctorViewModel
            {
                ApplicationUserId = doctor.ApplicationUserId,
                FullName = doctor.ApplicationUser.FullName,
                Gender = doctor.ApplicationUser.Gender,
                Email = doctor.ApplicationUser.Email,
                SpecialistIn = doctor.SpecialistIn,
                Status = doctor.Status
            };
        }

        public async Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorViewModel model)
        {
            var doctor = await _context.Doctors.Include(d => d.ApplicationUser)
                                               .FirstOrDefaultAsync(d => d.ApplicationUserId == model.ApplicationUserId);
            if (doctor == null)
                return (false, "Doctor not found");

            var user = doctor.ApplicationUser!;
            user.FullName = model.FullName;
            user.Gender = model.Gender;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, string.Join("<br/>", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!passResult.Succeeded)
                    return (false, string.Join("<br/>", passResult.Errors.Select(e => e.Description)));
            }

            doctor.SpecialistIn = model.SpecialistIn;
            doctor.Status = model.Status;

            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();

            return (true, "Doctor updated successfully");
        }

        public async Task<(bool Success, string Message)> DeleteDoctorAsync(string id)
        {
            var doctor = await _context.Doctors.Include(d => d.ApplicationUser)
                                               .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

            if (doctor == null)
                return (false, "Doctor not found");

            if (await _context.Appointments.AnyAsync(a => a.DoctorId == doctor.Id))
                return (false, "Cannot delete doctor with existing appointments");

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(doctor.ApplicationUser);
            if (!result.Succeeded)
                return (false, string.Join("<br/>", result.Errors.Select(e => e.Description)));

            return (true, "Doctor deleted successfully");
        }

        public async Task<List<string>> GetSpecialistListAsync()
        {
            return await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.SpecialistIn))
                .Select(d => d.SpecialistIn)
                .Distinct()
                .ToListAsync();
        }
    }
}
