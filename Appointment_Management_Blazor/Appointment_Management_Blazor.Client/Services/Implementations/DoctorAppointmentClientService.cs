using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class DoctorAppointmentClientService : IDoctorAppointmentClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public DoctorAppointmentClientService(HttpClient httpClient, ILocalStorageService localStorage)
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
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<AppointmentListResponse> GetAllAppointmentsAsync(AppointmentFilterModel filter)
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.PostAsJsonAsync("api/DoctorAppointment/list", filter);

            if (!response.IsSuccessStatusCode)
            {
                return new AppointmentListResponse
                {
                    Data = new(),
                    Draw = filter.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Error = $"Server error: {response.ReasonPhrase}"
                };
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var result = new AppointmentListResponse
            {
                Data = new(),
                Draw = filter.Draw,
                RecordsFiltered = 0,
                RecordsTotal = 0
            };

            if (json.TryGetProperty("data", out var dataElement))
            {
                result.Data = JsonSerializer.Deserialize<List<AppointmentDto>>(dataElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<AppointmentDto>();
            }

            if (json.TryGetProperty("recordsTotal", out var recordsTotalElement))
            {
                result.RecordsTotal = recordsTotalElement.GetInt32();
            }

            if (json.TryGetProperty("recordsFiltered", out var recordsFilteredElement))
            {
                result.RecordsFiltered = recordsFilteredElement.GetInt32();
            }
            else
            {
                result.RecordsFiltered = result.RecordsTotal;
            }

            if (json.TryGetProperty("draw", out var drawElement))
            {
                result.Draw = drawElement.GetInt32();
            }

            if (json.TryGetProperty("error", out var errorElement))
            {
                result.Error = errorElement.GetString();
            }

            return result;
        }

        public async Task<AppointmentViewModel?> GetAppointmentByIdAsync(int id)
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.GetAsync($"api/DoctorAppointment/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AppointmentViewModel>>();
            return apiResponse?.Data;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(UpdateStatusModel model)
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.PutAsJsonAsync("api/DoctorAppointment/status", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<DoctorInfoDto?> GetCurrentDoctorInfoAsync()
        {
            await AddJwtTokenAsync();

            var response = await _httpClient.GetAsync("api/DoctorAppointment/current-doctor");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<DoctorInfoDto>();
        }
    }
}
