using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Interfaces.Interfaces
{
    public interface IDoctorService
    {
        Task<DataStatsDto> GetDoctorStatsAsync();
        Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters);
        Task<List<string>> GetSpecialistListAsync();
        Task<DoctorViewModel?> GetDoctorByIdAsync(string id);
        Task<(bool Success, string Message)> CreateDoctorAsync(DoctorViewModel model);
        Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorViewModel model);
        Task<(bool Success, string Message)> DeleteDoctorAsync(string id);
    }
}
