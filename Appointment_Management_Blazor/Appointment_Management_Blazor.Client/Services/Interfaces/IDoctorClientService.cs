﻿using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IDoctorClientService
    {
        Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters);
        Task<DoctorDto> GetDoctorByIdAsync(string id);
        Task<ApiResponse> CreateDoctorAsync(DoctorViewModel model);
        Task<ApiResponse> UpdateDoctorAsync(DoctorViewModel model);
        Task<ApiResponse> DeleteDoctorAsync(string id);
        Task<List<string>> GetSpecialistListAsync();
    }
}
