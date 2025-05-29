
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<object> GetAllDoctorsAsync(DoctorFilterModel filters);
        Task<List<string>> GetSpecialistListAsync();
        Task<DoctorViewModel?> GetDoctorByIdAsync(string id);
        Task<(bool Success, string Message)> CreateDoctorAsync(DoctorViewModel model);
        Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorViewModel model);
        Task<(bool Success, string Message)> DeleteDoctorAsync(string id);
    }
}
