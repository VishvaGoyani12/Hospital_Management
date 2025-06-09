
using Appointment_Management_Blazor.EntityFrameworkCore.Data;
using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace Appointment_Management_Blazor.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;

        public PatientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataStatsDto> GetPatientStatsAsync()
        {
            var total = await _context.Patients.CountAsync();
            var active = await _context.Patients.CountAsync(d => d.Status);
            var inactive = await _context.Patients.CountAsync(d => !d.Status);

            return new DataStatsDto
            {
                TotalData = total,
                ActiveData = active,
                InactiveData = inactive
            };
        }

        public async Task<object> GetAllPatientsAsync(PatientFilterModel filter)
        {
            var query = _context.Patients.Include(p => p.ApplicationUser).AsQueryable();

            // Search (existing code remains the same)
            if (!string.IsNullOrEmpty(filter.SearchValue))
            {
                query = query.Where(p =>
                    (p.ApplicationUser.FullName != null && p.ApplicationUser.FullName.Contains(filter.SearchValue)) ||
                    (p.ApplicationUser.Gender != null && p.ApplicationUser.Gender.Contains(filter.SearchValue)) ||
                    p.JoinDate.ToString().Contains(filter.SearchValue) ||
                    (p.Status ? "Active" : "Deactive").Contains(filter.SearchValue)
                );
            }

            // Filters (existing code remains the same)
            if (!string.IsNullOrEmpty(filter.Gender))
                query = query.Where(p => p.ApplicationUser.Gender == filter.Gender);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.JoinDate.HasValue)
                query = query.Where(p => EF.Functions.DateDiffDay(p.JoinDate, filter.JoinDate.Value) == 0);

            var recordsTotal = await query.CountAsync();

            // Sorting (existing code remains the same)
            if (!string.IsNullOrEmpty(filter.SortColumn) && !string.IsNullOrEmpty(filter.SortDirection))
            {
                if (filter.SortColumn == "name") filter.SortColumn = "ApplicationUser.FullName";
                else if (filter.SortColumn == "gender") filter.SortColumn = "ApplicationUser.Gender";

                query = query.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            }

            var data = await query.Skip(filter.Start).Take(filter.Length)
                .Select(p => new
                {
                    p.Id,
                    FullName = p.ApplicationUser.FullName,
                    Gender = p.ApplicationUser.Gender,
                    p.JoinDate,
                    p.Status,
                    p.ProfileImagePath
                })
                .ToListAsync();

            return new
            {
                draw = filter.Draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = data.Select(p => new
                {
                    p.Id,
                    p.FullName,
                    p.Gender,
                    JoinDate = p.JoinDate?.ToString("yyyy-MM-dd"),
                    p.Status,
                    ProfileImagePath = p.ProfileImagePath != null ?
                        $"/{p.ProfileImagePath.Replace("\\", "/")}" :
                        "/images/default-profile.png"
                })
            };
        }

        public async Task<PatientViewModel?> GetPatientByIdAsync(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return null;

            return new PatientViewModel
            {
                Id = patient.Id,
                FullName = patient.ApplicationUser?.FullName ?? "",
                Gender = patient.ApplicationUser?.Gender ?? "",
                JoinDate = patient.JoinDate,
                Status = patient.Status,
                ProfileImagePath = patient.ProfileImagePath 
            };
        }

        public async Task<bool> UpdatePatientAsync(PatientViewModel model)
        {
            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (patient == null) return false;

            patient.Status = model.Status;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(bool Success, string Message)> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return (false, "Patient not found");

            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.PatientId == patient.Id &&
                              (a.Status == "Pending" || a.Status == "Confirmed"));

            if (hasAppointments)
                return (false, "Cannot delete patient with existing pending or confirmed appointments");

            if (patient.ApplicationUser != null)
            {
                _context.Users.Remove(patient.ApplicationUser); 
            }

            _context.Patients.Remove(patient); 
            await _context.SaveChangesAsync();

            return (true, "Patient deleted successfully");
        }

    }
}
