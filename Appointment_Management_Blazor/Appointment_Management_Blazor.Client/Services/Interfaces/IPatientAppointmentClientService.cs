using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IPatientAppointmentClientService
    {
        Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters);
        Task<AppointmentDto> GetAppointmentByIdAsync(int id);
        Task<ApiResponse> CreateAppointmentAsync(AppointmentViewModel model);
        Task<ApiResponse> UpdateAppointmentAsync(AppointmentViewModel model);
        Task<ApiResponse> DeleteAppointmentAsync(int id);
        Task<List<DoctorDropdownDto>> GetAvailableDoctorsAsync(DateTime? date, int? selectedDoctorId);
    }
}
