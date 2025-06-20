﻿using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IPatientAppointmentClientService
    {
        Task<AppointmentStatsDto?> GetAppointmentStatsAsync();
        Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters);
        Task<AppointmentViewModel> GetAppointmentByIdAsync(int id);
        Task<ApiResponse> CreateAppointmentAsync(AppointmentViewModel model);
        Task<ApiResponse> UpdateAppointmentAsync(AppointmentViewModel model);
        Task<ApiResponse> DeleteAppointmentAsync(int id);
        Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? date, int? selectedDoctorId = null);
        Task<PatientInfoDto> GetCurrentPatientInfo();
    }
}
