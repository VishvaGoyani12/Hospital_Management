using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IDoctorAppointmentClientService
    {
        Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filter);
        Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id);
        Task<bool> UpdateAppointmentStatusAsync(UpdateStatusModel model);
        Task<DoctorInfoDto?> GetCurrentDoctorInfoAsync();
    }
}
