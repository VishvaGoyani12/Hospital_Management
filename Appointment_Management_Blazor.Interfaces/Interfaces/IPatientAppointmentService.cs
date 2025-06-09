using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Interfaces.Interfaces
{
    public interface IPatientAppointmentService
    {
        Task<AppointmentStatsDto> GetAppointmentStatsAsync(int? patientId = null);
        Task<(int TotalCount, List<AppointmentViewModel> Data)> GetAllAppointmentsAsync(AppointmentFilterModel filters);
        Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAppointmentAsync(AppointmentViewModel model);
        Task<(bool Success, string Message)> UpdateAppointmentAsync(AppointmentViewModel model);
        Task<(bool Success, string Message)> DeleteAppointmentAsync(int id);
        Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? selectedDate, int? selectedDoctorId = null);
        Task<int> GetPatientIdByUserId(string userId);
    }
}
