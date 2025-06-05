using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class PatientAppointmentClientService : IPatientAppointmentClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public PatientAppointmentClientService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        private async Task AddJwtTokenAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("jwt_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.Replace("\"", "");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filters)
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.PostAsJsonAsync("api/PatientAppointment/list", filters);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AppointmentListResponse>();
        }

        public async Task<List<DoctorViewModel>> GetAvailableDoctorsAsync(DateTime? date, int? selectedDoctorId = null)
        {
            await AddJwtTokenAsync();

            try
            {
                var queryParams = new List<string>();

                if (date.HasValue)
                    queryParams.Add($"date={date.Value:yyyy-MM-dd}");
                if (selectedDoctorId.HasValue)
                    queryParams.Add($"selectedDoctorId={selectedDoctorId.Value}");

                var url = "api/PatientAppointment/available-doctors";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Server error: {response.StatusCode} - {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<List<DoctorViewModel>>();
            }
            catch
            {
                throw;
            }
        }

        public async Task<PatientInfoDto> GetCurrentPatientInfo()
        {
            await AddJwtTokenAsync();
            return await _httpClient.GetFromJsonAsync<PatientInfoDto>("api/PatientAppointment/current-patient");
        }

        public async Task<AppointmentViewModel> GetAppointmentByIdAsync(int id)
        {
            await AddJwtTokenAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/PatientAppointment/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Server error: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw API response: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<AppointmentViewModel>>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (apiResponse?.Success != true)
                {
                    throw new Exception(apiResponse?.Message ?? "Failed to get appointment");
                }

                return apiResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting appointment: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResponse> CreateAppointmentAsync(AppointmentViewModel model)
        {
            await AddJwtTokenAsync();
            var response = await _httpClient.PostAsJsonAsync("api/PatientAppointment", model);
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<ApiResponse> UpdateAppointmentAsync(AppointmentViewModel model)
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.PutAsJsonAsync("api/PatientAppointment", model);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return result!;
        }


        public async Task<ApiResponse> DeleteAppointmentAsync(int id)
        {
            await AddJwtTokenAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/PatientAppointment/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode} - {errorContent}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return result ?? new ApiResponse { Success = false, Message = "Unknown response from server" };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
