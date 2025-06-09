using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Interfaces.Interfaces
{
    public interface IPatientService
    {
        Task<DataStatsDto> GetPatientStatsAsync();
        Task<object> GetAllPatientsAsync(PatientFilterModel filter);
        Task<PatientViewModel?> GetPatientByIdAsync(int id);
        Task<bool> UpdatePatientAsync(PatientViewModel model);
        Task<(bool Success, string Message)> DeletePatientAsync(int id); 
    }
}
