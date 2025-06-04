using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Services.Interfaces
{
    public interface IPatientAppointmentService
    {
        Task<(int TotalCount, List<AppointmentViewModel> Data)> GetAllAppointmentsAsync(AppointmentFilterModel filters);
        Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAppointmentAsync(AppointmentViewModel model);
        Task<(bool Success, string Message)> UpdateAppointmentAsync(AppointmentViewModel model);
        Task<(bool Success, string Message)> DeleteAppointmentAsync(int id);
        Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? selectedDate, int? selectedDoctorId = null);
        Task<int> GetPatientIdByUserId(string userId);
    }
}
