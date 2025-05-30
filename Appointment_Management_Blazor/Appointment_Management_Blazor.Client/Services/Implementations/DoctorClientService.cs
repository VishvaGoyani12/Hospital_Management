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
                var queryParams = new Dictionary<string, string>
                {
                    ["draw"] = filters.Draw.ToString(),
                    ["start"] = filters.Start.ToString(),
                    ["length"] = filters.Length.ToString()
                };

                if (!string.IsNullOrEmpty(filters.SearchValue))
                    queryParams["searchValue"] = filters.SearchValue;

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                var response = await _httpClient.GetAsync($"api/doctor?{queryString}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<DoctorListResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching doctors: {ex.Message}");
                return new DoctorListResponse();
            }
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<DoctorDto>($"api/doctor/{id}");
        }

        public async Task<ApiResponse> CreateDoctorAsync(DoctorViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/doctor/create", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return result ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
        }

        public async Task<ApiResponse> UpdateDoctorAsync(DoctorViewModel model)
        {
            // Assuming your API expects PUT at api/doctors/{id}
            var response = await _httpClient.PutAsJsonAsync("api/doctor/edit", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return result ?? new ApiResponse { Success = false, Message = "Unknown error" };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Message = error };
            }
        }


        public async Task<ApiResponse> DeleteDoctorAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"api/doctor/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }

        public async Task<List<string>> GetSpecialistListAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("api/doctor/specialists");
        }
    }
}