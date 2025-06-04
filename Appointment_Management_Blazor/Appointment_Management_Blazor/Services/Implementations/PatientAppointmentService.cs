// PatientAppointmentService.cs
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
    public class PatientAppointmentService : IPatientAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientAppointmentService(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(int TotalCount, List<AppointmentViewModel> Data)> GetAllAppointmentsAsync(AppointmentFilterModel filters)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.ApplicationUser)
                    .Include(a => a.Doctor).ThenInclude(d => d.ApplicationUser)
                    .AsQueryable();

                if (filters.PatientId.HasValue)
                {
                    query = query.Where(a => a.PatientId == filters.PatientId.Value);
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    query = query.Where(a => a.Status == filters.Status);
                }

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

                // Handle sorting
                if (!string.IsNullOrEmpty(filters.SortColumn) && !string.IsNullOrEmpty(filters.SortDirection))
                {
                    // Special handling for navigation properties
                    if (filters.SortColumn == "PatientName")
                    {
                        query = filters.SortDirection == "asc"
                            ? query.OrderBy(a => a.Patient.ApplicationUser.FullName)
                            : query.OrderByDescending(a => a.Patient.ApplicationUser.FullName);
                    }
                    else if (filters.SortColumn == "DoctorName")
                    {
                        query = filters.SortDirection == "asc"
                            ? query.OrderBy(a => a.Doctor.ApplicationUser.FullName)
                            : query.OrderByDescending(a => a.Doctor.ApplicationUser.FullName);
                    }
                    else
                    {
                        // For other columns, use dynamic sorting
                        query = query.OrderBy($"{filters.SortColumn} {filters.SortDirection}");
                    }
                }
                else
                {
                    query = query.OrderByDescending(a => a.AppointmentDate);
                }

                var data = await query
                    .Skip(filters.Start)
                    .Take(filters.Length)
                    .Select(a => new AppointmentViewModel
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        Description = a.Description,
                        Status = a.Status,
                        DoctorId = a.DoctorId,
                        PatientId = a.PatientId,
                        PatientName = a.Patient.ApplicationUser.FullName,
                        DoctorName = a.Doctor.ApplicationUser.FullName
                    })
                    .ToListAsync();

                return (total, data);
            }
            catch (Exception ex)
            {
                // Log error here
                throw;
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
                // Log error here
                throw;
            }
        }


        public async Task<(bool Success, string Message)> CreateAppointmentAsync(AppointmentViewModel vm)
        {
            try
            {
                if (vm.PatientId <= 0)
                {
                    return (false, "Patient ID is required.");
                }
                var patient = await _context.Patients.FindAsync(vm.PatientId);
                if (patient == null)
                {
                    return (false, "Patient profile not found.");
                }

                if (vm.AppointmentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    return (false, "Appointments cannot be booked on Sundays.");
                }

                if (vm.AppointmentDate < DateTime.Now)
                {
                    return (false, "Appointment date cannot be in the past.");
                }

                var appointmentHour = vm.AppointmentDate.Hour;
                if (appointmentHour < 9 || appointmentHour >= 17)
                {
                    return (false, "Appointment must be within doctor's working hours (9 AM - 5 PM).");
                }

                var fiveDaysAgo = vm.AppointmentDate.Date.AddDays(-5);
                bool hasRecentSameDoctorAppointment = await _context.Appointments.AnyAsync(a =>
                    a.PatientId == vm.PatientId &&
                    a.DoctorId == vm.DoctorId &&
                    a.AppointmentDate.Date >= fiveDaysAgo &&
                    a.AppointmentDate.Date < vm.AppointmentDate.Date);

                if (hasRecentSameDoctorAppointment)
                {
                    return (false, "You cannot book with the same doctor again within 5 days.");
                }

                bool hasOtherDoctorSameDate = await _context.Appointments.AnyAsync(a =>
                    a.PatientId == vm.PatientId &&
                    a.AppointmentDate.Date == vm.AppointmentDate.Date &&
                    a.DoctorId != vm.DoctorId);

                if (hasOtherDoctorSameDate)
                {
                    return (false, "You already have an appointment with another doctor on this date.");
                }

                int maxAppointmentsPerDay = 10;
                int doctorAppointmentsCount = await _context.Appointments.CountAsync(a =>
                    a.DoctorId == vm.DoctorId &&
                    a.AppointmentDate.Date == vm.AppointmentDate.Date);

                if (doctorAppointmentsCount >= maxAppointmentsPerDay)
                {
                    return (false, "The doctor is fully booked for this date.");
                }

                bool hasOverlappingAppointment = await _context.Appointments.AnyAsync(a =>
                    a.PatientId == vm.PatientId &&
                    a.DoctorId == vm.DoctorId &&
                    a.AppointmentDate == vm.AppointmentDate);

                if (hasOverlappingAppointment)
                {
                    return (false, "You already have an appointment with this doctor at the selected time.");
                }

                var appointment = new Appointment
                {
                    PatientId = vm.PatientId,
                    DoctorId = vm.DoctorId,
                    AppointmentDate = vm.AppointmentDate,
                    Description = vm.Description,
                    Status = "Pending"
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return (true, "Appointment created successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error creating appointment: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAppointmentAsync(AppointmentViewModel vm)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(vm.Id);
                if (appointment == null)
                {
                    return (false, "Appointment not found.");
                }

                // Don't allow changing patient ID
                if (vm.PatientId != appointment.PatientId)
                {
                    return (false, "Cannot change patient for an existing appointment.");
                }

                bool isDuplicate = await _context.Appointments.AnyAsync(a =>
                    a.Id != vm.Id &&
                    a.DoctorId == vm.DoctorId &&
                    a.PatientId == appointment.PatientId &&
                    a.AppointmentDate.Date == vm.AppointmentDate.Date);

                if (isDuplicate)
                {
                    return (false, "This appointment already exists.");
                }

                // Only update these fields - don't change status
                appointment.DoctorId = vm.DoctorId;
                appointment.AppointmentDate = vm.AppointmentDate;
                appointment.Description = vm.Description;

                await _context.SaveChangesAsync();
                return (true, "Appointment updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating appointment: {ex.Message}");
            }
        }


        public async Task<(bool Success, string Message)> DeleteAppointmentAsync(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return (false, "Appointment not found.");
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return (true, "Appointment deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting appointment: {ex.Message}");
            }
        }


        public async Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? selectedDate, int? selectedDoctorId)
        {
            try
            {
                if (selectedDate.HasValue && selectedDate.Value == DateTime.MinValue)
                {
                    return new List<DoctorViewModel>();
                }

                var bookedDoctorIds = selectedDate.HasValue
                    ? await _context.Appointments
                        .Where(a => a.AppointmentDate.Date == selectedDate.Value.Date)
                        .Select(a => a.DoctorId)
                        .ToListAsync()
                    : new List<int>();

                return await _context.Doctors
                    .Include(d => d.ApplicationUser)
                    .Where(d => d.Status && (!selectedDate.HasValue || !bookedDoctorIds.Contains(d.Id) || d.Id == selectedDoctorId))
                    .Select(d => new DoctorViewModel
                    {
                        Id = d.Id,
                        FullName = d.ApplicationUser.FullName,
                        SpecialistIn = d.SpecialistIn,
                        Status = d.Status
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error here
                throw;
            }
        }


        public async Task<int> GetPatientIdByUserId(string userId)
        {
            try
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
                return patient?.Id ?? 0;
            }
            catch (Exception ex)
            {
                // Log error here
                throw;
            }
        }
    }
}