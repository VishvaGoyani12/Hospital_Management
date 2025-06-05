using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class DoctorClientService : IDoctorClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public DoctorClientService(HttpClient httpClient, ILocalStorageService localStorage)
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

    public async Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters)
    {
        try
        {
            await AddJwtTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, "api/doctor/list")
            {
                Content = JsonContent.Create(filters)
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DoctorListResponse>();
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
        await AddJwtTokenAsync();
        var response = await _httpClient.GetAsync($"api/doctor/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DoctorDto>();
        }
        return null;
    }

    public async Task<ApiResponse> CreateDoctorAsync(DoctorViewModel model)
    {
        try
        {
            await AddJwtTokenAsync();
            var response = await _httpClient.PostAsJsonAsync("api/doctor/create", model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse { Success = false, Message = $"Error: {response.StatusCode} - {errorContent}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = $"Exception: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> UpdateDoctorAsync(DoctorViewModel model)
    {
        try
        {
            await AddJwtTokenAsync();
            var response = await _httpClient.PutAsJsonAsync("api/doctor/edit", model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse { Success = false, Message = $"Error: {response.StatusCode} - {errorContent}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = $"Exception: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> DeleteDoctorAsync(string id)
    {
        try
        {
            await AddJwtTokenAsync();
            var response = await _httpClient.DeleteAsync($"api/doctor/{id}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse { Success = false, Message = $"Error: {response.StatusCode} - {errorContent}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = $"Exception: {ex.Message}" };
        }
    }

    public async Task<List<string>> GetSpecialistListAsync()
    {
        await AddJwtTokenAsync();
        return await _httpClient.GetFromJsonAsync<List<string>>("api/doctor/specialists");
    }
}
