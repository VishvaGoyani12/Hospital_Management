
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
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

    public async Task<DataStatsDto> GetDoctorStatsAsync()
    {
        try
        {
            await AddJwtTokenAsync();
            return await _httpClient.GetFromJsonAsync<DataStatsDto>("api/doctor/stats");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching doctor stats: {ex.Message}");
            return new DataStatsDto();
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

    public async Task<ApiResponse> CreateDoctorAsync(DoctorCreateEditModel model)
    {
        try
        {
            await AddJwtTokenAsync();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.FullName ?? ""), nameof(model.FullName));
            content.Add(new StringContent(model.Gender ?? ""), nameof(model.Gender));
            content.Add(new StringContent(model.Email ?? ""), nameof(model.Email));
            content.Add(new StringContent(model.SpecialistIn ?? ""), nameof(model.SpecialistIn));
            content.Add(new StringContent(model.Status.ToString()), nameof(model.Status));

            if (!string.IsNullOrEmpty(model.Password))
            {
                content.Add(new StringContent(model.Password), nameof(model.Password));
                content.Add(new StringContent(model.ConfirmPassword ?? ""), nameof(model.ConfirmPassword));
            }

            if (model.ProfileImage != null)
            {
                // Limit max stream size and open stream
                var stream = model.ProfileImage.OpenReadStream(1024 * 1024 * 10); // 10 MB max
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfileImage.ContentType);

                // "ProfileImage" must match server property name exactly
                content.Add(fileContent, nameof(model.ProfileImage), model.ProfileImage.Name);
            }

            var response = await _httpClient.PostAsync("api/doctor/create", content);

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


    public async Task<ApiResponse> UpdateDoctorAsync(DoctorCreateEditModel model)
    {
        try
        {
            await AddJwtTokenAsync();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.ApplicationUserId ?? ""), "ApplicationUserId");
            content.Add(new StringContent(model.FullName ?? ""), "FullName");
            content.Add(new StringContent(model.Gender ?? ""), "Gender");
            content.Add(new StringContent(model.Email ?? ""), "Email");
            content.Add(new StringContent(model.SpecialistIn ?? ""), "SpecialistIn");
            content.Add(new StringContent(model.Status.ToString()), "Status");

            if (!string.IsNullOrEmpty(model.Password))
            {
                content.Add(new StringContent(model.Password), "Password");
                content.Add(new StringContent(model.ConfirmPassword), "ConfirmPassword");
            }

            if (model.ProfileImage != null)
            {
                var fileContent = new StreamContent(model.ProfileImage.OpenReadStream(1024 * 1024 * 10)); 
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfileImage.ContentType);
                content.Add(fileContent, "ProfileImage", model.ProfileImage.Name);
            }

            var response = await _httpClient.PutAsync("api/doctor/edit", content);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse>();

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse { Success = false, Message = errorContent.Trim('"') };
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
