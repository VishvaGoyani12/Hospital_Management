using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Services.Interfaces
{
    public interface IPatientService
    {
        Task<object> GetAllPatientsAsync(PatientFilterModel filter);
        Task<PatientViewModel?> GetPatientByIdAsync(int id);
        Task<bool> UpdatePatientAsync(PatientViewModel model);
        Task<(bool Success, string Message)> DeletePatientAsync(int id); 
    }
}
