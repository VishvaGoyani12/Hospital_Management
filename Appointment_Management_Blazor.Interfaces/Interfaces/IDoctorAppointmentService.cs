using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Interfaces.Interfaces
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
