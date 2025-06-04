using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Services.Interfaces
{
    public interface IDoctorAppointmentService
    {
        Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters);
        Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateAppointmentStatusAsync(UpdateStatusModel model);
        Task<int> GetDoctorIdByUserId(string userId);
        Task<DoctorViewModel?> GetDoctorByIdAsync(int id);
    }
}
