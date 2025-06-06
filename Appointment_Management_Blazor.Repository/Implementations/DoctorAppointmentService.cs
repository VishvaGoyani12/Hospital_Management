using Appointment_Management_Blazor.EntityFrameworkCore.Data;
using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace Appointment_Management_Blazor.Repository.Implementations
{
    public class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorAppointmentService(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
                    .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
                    .AsQueryable();

                if (filters.DoctorId.HasValue)
                    query = query.Where(a => a.DoctorId == filters.DoctorId.Value);

                if (!string.IsNullOrEmpty(filters.Status))
                    query = query.Where(a => a.Status == filters.Status);

                if (!string.IsNullOrEmpty(filters.SearchValue))
                {
                    var search = filters.SearchValue.ToLower();
                    query = query.Where(a =>
                        a.Description.ToLower().Contains(search) ||
                        a.Status.ToLower().Contains(search) ||
                        a.AppointmentDate.ToString().ToLower().Contains(search) ||
                        a.Patient.ApplicationUser.FullName.ToLower().Contains(search) ||
                        a.Doctor.ApplicationUser.FullName.ToLower().Contains(search));
                }

                var total = await query.CountAsync();

                if (!string.IsNullOrEmpty(filters.SortColumn) && !string.IsNullOrEmpty(filters.SortDirection))
                {
                    query = query.OrderBy($"{filters.SortColumn} {filters.SortDirection}");
                }
                else
                {
                    query = query.OrderByDescending(a => a.AppointmentDate);
                }

                var data = await query
                    .Skip(filters.Start)
                    .Take(filters.Length)
                    .ToListAsync();

                var appointmentDtos = data.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Description = a.Description,
                    Status = a.Status,
                    DoctorId = a.DoctorId,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.ApplicationUser.FullName,
                    DoctorName = a.Doctor.ApplicationUser.FullName
                }).ToList();

                return new AppointmentListResponse
                {
                    Draw = filters.Draw,
                    RecordsTotal = total,
                    RecordsFiltered = total,
                    Data = appointmentDtos
                };
            }
            catch (Exception ex)
            {
                return new AppointmentListResponse
                {
                    Data = new List<AppointmentDto>(),
                    Error = ex.Message,
                    Draw = filters.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0
                };
            }
        }


        public async Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
                    .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
                    .Where(a => a.Id == id)
                    .Select(a => new AppointmentViewModel
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        DoctorId = a.DoctorId,
                        AppointmentDate = a.AppointmentDate,
                        Description = a.Description,
                        Status = a.Status,
                        PatientName = a.Patient.ApplicationUser.FullName,
                        DoctorName = a.Doctor.ApplicationUser.FullName
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<(bool Success, string Message)> UpdateAppointmentStatusAsync(UpdateStatusModel model)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(model.Id);
                if (appointment == null)
                {
                    return (false, "Appointment not found.");
                }

                appointment.Status = model.Status;
                await _context.SaveChangesAsync();
                return (true, "Appointment status updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating appointment status: {ex.Message}");
            }
        }

        public async Task<int> GetDoctorIdByUserId(string userId)
        {
            try
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
                return doctor?.Id ?? 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<DoctorViewModel?> GetDoctorByIdAsync(int id)
        {
            try
            {
                return await _context.Doctors
                    .Include(d => d.ApplicationUser)
                    .Where(d => d.Id == id)
                    .Select(d => new DoctorViewModel
                    {
                        Id = d.Id,
                        FullName = d.ApplicationUser.FullName,
                        SpecialistIn = d.SpecialistIn,
                        Status = d.Status
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
