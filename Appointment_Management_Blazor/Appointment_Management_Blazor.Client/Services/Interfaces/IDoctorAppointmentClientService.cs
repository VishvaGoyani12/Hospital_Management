
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

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
