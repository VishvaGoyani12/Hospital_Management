using Appointment_Management_Blazor.Client.Helper;
using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Blazored.LocalStorage;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class DoctorClientService : IDoctorClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public DoctorClientService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }


        public async Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/doctor/list", filters);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DoctorListResponse>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new DoctorListResponse
                {
                    Data = new List<DoctorDto>(),
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Draw = filters.Draw
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching doctors: {ex.Message}");
                return new DoctorListResponse
                {
                    Data = new List<DoctorDto>(),
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Draw = filters.Draw
                };
            }
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"api/doctor/{id}");
            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON to DoctorDto
                var doctor = await response.Content.ReadFromJsonAsync<DoctorDto>();
                return doctor;
            }
            return null;
        }


        public async Task<ApiResponse> CreateDoctorAsync(DoctorViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/doctor/create", model); // Changed from "api/doctor/create" to "api/account/create"

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

        public async Task<ApiResponse> UpdateDoctorAsync(DoctorViewModel model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/doctor/edit", model);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update error: {errorContent}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode} - {errorContent}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update exception: {ex}");
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> DeleteDoctorAsync(string id)
        {
            try
            {
                // Changed endpoint to match controller route
                var response = await _httpClient.DeleteAsync($"api/doctor/{id}");

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

        public async Task<List<string>> GetSpecialistListAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("api/doctor/specialists");
        }
    }
}