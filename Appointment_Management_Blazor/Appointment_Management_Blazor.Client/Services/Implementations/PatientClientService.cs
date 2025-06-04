using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Blazored.LocalStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class PatientClientService : IPatientClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public PatientClientService(HttpClient httpClient, ILocalStorageService localStorage)
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

        public async Task<PatientListResponse> GetAllPatientsAsync(PatientFilterModel filters)
        {
            try
            {
                await AddJwtTokenAsync();

                var query = $"api/patient/GetAll?Draw={filters.Draw}&Start={filters.Start}&Length={filters.Length}&SearchValue={filters.SearchValue}";

                if (!string.IsNullOrEmpty(filters.Gender))
                    query += $"&Gender={filters.Gender}";
                if (filters.Status.HasValue)
                    query += $"&Status={filters.Status}";
                if (filters.JoinDate.HasValue)
                    query += $"&JoinDate={filters.JoinDate:yyyy-MM-dd}";
                if (!string.IsNullOrEmpty(filters.SortColumn))
                    query += $"&SortColumn={filters.SortColumn}";
                if (!string.IsNullOrEmpty(filters.SortDirection))
                    query += $"&SortDirection={filters.SortDirection}";

                var response = await _httpClient.GetAsync(query);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PatientListResponse>();
                }

                return new PatientListResponse
                {
                    Data = new List<PatientDto>(),
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Draw = filters.Draw
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching patients: {ex.Message}");
                return new PatientListResponse
                {
                    Data = new List<PatientDto>(),
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Draw = filters.Draw
                };
            }
        }

        public async Task<PatientDto> GetPatientByIdAsync(int id)
        {
            try
            {
                await AddJwtTokenAsync();

                var response = await _httpClient.GetAsync($"api/patient/Edit/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PatientDto>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error getting patient: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting patient: {ex}");
                return null;
            }
        }

        public async Task<ApiResponse> UpdatePatientAsync(PatientViewModel model)
        {
            try
            {
                await AddJwtTokenAsync();

                var response = await _httpClient.PostAsJsonAsync("api/patient/Edit", model);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> DeletePatientAsync(int id)
        {
            try
            {
                await AddJwtTokenAsync();

                var response = await _httpClient.DeleteAsync($"api/patient/Delete/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }
    }
}
