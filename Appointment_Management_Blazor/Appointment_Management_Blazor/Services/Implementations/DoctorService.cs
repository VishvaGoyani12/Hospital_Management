using Appointment_Management_Blazor.Client.Models.DTOs;
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

        // Services/DoctorService.cs
        public async Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters)
        {
            var query = _context.Doctors
                .Include(d => d.ApplicationUser)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(filters.SearchValue))
            {
                query = query.Where(d =>
                    d.ApplicationUser.FullName.Contains(filters.SearchValue) ||
                    d.SpecialistIn.Contains(filters.SearchValue) ||
                    d.ApplicationUser.Email.Contains(filters.SearchValue));
            }

            // Apply status filter if provided
            if (filters.Status.HasValue)
            {
                query = query.Where(d => d.Status == filters.Status.Value);
            }

            // Apply gender filter if provided
            if (!string.IsNullOrEmpty(filters.Gender))
            {
                query = query.Where(d => d.ApplicationUser.Gender == filters.Gender);
            }

            // Apply specialization filter if provided
            if (!string.IsNullOrEmpty(filters.SpecialistIn))
            {
                query = query.Where(d => d.SpecialistIn == filters.SpecialistIn);
            }

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(filters.SortColumn))
            {
                switch (filters.SortColumn)
                {
                    case "FullName":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.FullName)
                            : query.OrderBy(d => d.ApplicationUser.FullName);
                        break;
                    case "Gender":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.Gender)
                            : query.OrderBy(d => d.ApplicationUser.Gender);
                        break;
                    case "Email":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.Email)
                            : query.OrderBy(d => d.ApplicationUser.Email);
                        break;
                    case "SpecialistIn":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.SpecialistIn)
                            : query.OrderBy(d => d.SpecialistIn);
                        break;
                    case "Status":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.Status)
                            : query.OrderBy(d => d.Status);
                        break;
                    default:
                        // fallback if unknown column
                        query = query.OrderBy(d => d.ApplicationUser.FullName);
                        break;
                }
            }


            // Apply pagination
            var doctors = await query
                .Skip(filters.Start)
                .Take(filters.Length)
                .Select(d => new DoctorDto
                {
                    Id = d.ApplicationUserId,
                    FullName = d.ApplicationUser.FullName,
                    Gender = d.ApplicationUser.Gender,
                    Email = d.ApplicationUser.Email,
                    SpecialistIn = d.SpecialistIn,
                    Status = d.Status
                })
                .ToListAsync();

            return new DoctorListResponse
            {
                Draw = filters.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = doctors
            };
        }


        public async Task<(bool Success, string Message)> CreateDoctorAsync(DoctorViewModel model)
        {
            // Validate password
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return (false, "Password and confirmation password do not match.");
            }

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
            var doctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

            if (doctor == null || doctor.ApplicationUser == null)
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
            try
            {
                if (string.IsNullOrEmpty(model.ApplicationUserId))
                {
                    return (false, "User ID is required");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
                if (user == null)
                {
                    // Log the ID that wasn't found
                    Console.WriteLine($"User not found with ID: {model.ApplicationUserId}");
                    return (false, $"User not found with ID: {model.ApplicationUserId}");
                }

                // Find doctor
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == model.ApplicationUserId);

                if (doctor == null)
                {
                    return (false, "Doctor not found");
                }

                // Update user properties
                user.FullName = model.FullName;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.NormalizedUserName = model.Email.ToUpper();

                var userResult = await _userManager.UpdateAsync(user);
                if (!userResult.Succeeded)
                {
                    return (false, string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }

                // Update password if provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (!passResult.Succeeded)
                    {
                        return (false, string.Join(", ", passResult.Errors.Select(e => e.Description)));
                    }
                }

                // Update doctor properties
                doctor.SpecialistIn = model.SpecialistIn;
                doctor.Status = model.Status;

                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();

                return (true, "Doctor updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating doctor: {ex}");
                return (false, $"Error updating doctor: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteDoctorAsync(string id)
        {
            var doctor = await _context.Doctors.Include(d => d.ApplicationUser)
                                               .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

            if (doctor == null)
                return (false, "Doctor not found");

            // Check for any appointments with status "Pending" or "Confirmed"
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctor.Id &&
                              (a.Status == "Pending" || a.Status == "Confirmed"));

            if (hasAppointments)
                return (false, "Cannot delete doctor with existing pending or confirmed appointments");

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
