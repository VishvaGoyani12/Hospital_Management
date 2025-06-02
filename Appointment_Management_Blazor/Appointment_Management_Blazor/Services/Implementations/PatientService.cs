using Appointment_Management_Blazor.Data;
using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
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

        public async Task<object> GetAllPatientsAsync(PatientFilterModel filter)
        {
            var query = _context.Patients.Include(p => p.ApplicationUser).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(filter.SearchValue))
            {
                query = query.Where(p =>
                    (p.ApplicationUser.FullName != null && p.ApplicationUser.FullName.Contains(filter.SearchValue)) ||
                    (p.ApplicationUser.Gender != null && p.ApplicationUser.Gender.Contains(filter.SearchValue)) ||
                    p.JoinDate.ToString().Contains(filter.SearchValue) ||
                    (p.Status ? "Active" : "Deactive").Contains(filter.SearchValue)
                );
            }

            // Filters
            if (!string.IsNullOrEmpty(filter.Gender))
                query = query.Where(p => p.ApplicationUser.Gender == filter.Gender);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.JoinDate.HasValue)
                query = query.Where(p => EF.Functions.DateDiffDay(p.JoinDate, filter.JoinDate.Value) == 0);
            var recordsTotal = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(filter.SortColumn) && !string.IsNullOrEmpty(filter.SortDirection))
            {
                if (filter.SortColumn == "name") filter.SortColumn = "ApplicationUser.FullName";
                else if (filter.SortColumn == "gender") filter.SortColumn = "ApplicationUser.Gender";

                query = query.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            }

            var data = query.Skip(filter.Start).Take(filter.Length)
    .Select(p => new
    {
        p.Id,
        FullName = p.ApplicationUser.FullName, 
        Gender = p.ApplicationUser.Gender,
        p.JoinDate,
        Status = p.Status
    })

    .AsEnumerable()
    .Select(p => new
    {
        p.Id,
        p.FullName,
        p.Gender,
        JoinDate = p.JoinDate?.ToString("yyyy-MM-dd"),
        p.Status 
    })
    .ToList();



            return new
            {
                draw = filter.Draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
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
                Status = patient.Status
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

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return false;

            // Optional: If you want to delete the associated ApplicationUser too
            if (patient.ApplicationUser != null)
            {
                _context.Users.Remove(patient.ApplicationUser); // Remove from Identity
            }

            _context.Patients.Remove(patient); // Remove from Patient table
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
