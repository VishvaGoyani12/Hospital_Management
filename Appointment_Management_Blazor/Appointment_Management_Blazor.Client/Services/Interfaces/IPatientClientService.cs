using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IPatientClientService
    {
        Task<PatientListResponse> GetAllPatientsAsync(PatientFilterModel filters);
        Task<PatientDto> GetPatientByIdAsync(int id);
        Task<ApiResponse> UpdatePatientAsync(PatientViewModel model);
        Task<(bool Success, string Message)> DeletePatientAsync(int id);
    }
}
