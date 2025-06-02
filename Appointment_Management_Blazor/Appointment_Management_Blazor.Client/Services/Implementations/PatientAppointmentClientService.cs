using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using System.Net.Http.Json;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class PatientAppointmentClientService : IPatientAppointmentClientService
    {
        private readonly HttpClient _httpClient;

        public PatientAppointmentClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Client/Services/Implementations/PatientAppointmentClientService.cs
        public async Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/PatientAppointment/list", filters);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Server error: {response.StatusCode} - {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<AppointmentListResponse>();
            }
            catch (Exception ex)
            {
                // Log error here
                throw;
            }
        }

        public async Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? date, int? selectedDoctorId = null)
        {
            try
            {
                // Ensure date has a valid value
                var queryDate = date ?? DateTime.Today;
                var url = $"api/PatientAppointment/available-doctors?date={queryDate:yyyy-MM-dd}";

                if (selectedDoctorId.HasValue)
                {
                    url += $"&selectedDoctorId={selectedDoctorId.Value}";
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Server error: {response.StatusCode} - {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<List<DoctorViewModel>>();
            }
            catch (Exception ex)
            {
                // Log error here
                throw;
            }
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<AppointmentDto>($"api/PatientAppointment/{id}");
        }

        public async Task<ApiResponse> CreateAppointmentAsync(AppointmentViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/PatientAppointment", model);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> UpdateAppointmentAsync(AppointmentViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync("api/PatientAppointment", model);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> DeleteAppointmentAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/PatientAppointment/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        
    }
}
