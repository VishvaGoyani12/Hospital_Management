
using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Blazored.LocalStorage;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Appointment_Management_Blazor.Client.Services.Implementations
{
    public class AccountClientService : IAccountClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AccountClientService(HttpClient httpClient, ILocalStorageService localStorage)
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

        public async Task<AuthResponse> RegisterAsync(ClientRegisterModel model)
        {
            try
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(model.FullName ?? ""), "FullName");
                formData.Add(new StringContent(model.Email ?? ""), "Email");
                formData.Add(new StringContent(model.Gender ?? ""), "Gender");
                formData.Add(new StringContent(model.JoinDate.ToString("yyyy-MM-dd")), "JoinDate");
                formData.Add(new StringContent(model.Password ?? ""), "Password");
                formData.Add(new StringContent(model.ConfirmPassword ?? ""), "ConfirmPassword");

                if (model.ProfileImage != null)
                {
                    var stream = model.ProfileImage.OpenReadStream(1024 * 1024 * 5); 
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfileImage.ContentType);

                    formData.Add(fileContent, "ProfileImage", model.ProfileImage.Name);
                }

                var response = await _httpClient.PostAsync("api/account/register", formData);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "Empty response from server"
                    };
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = content ?? $"Registration failed with status: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"Client error during registration: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/account/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                    {
                        return new AuthResponse
                        {
                            IsSuccess = false,
                            Message = errorResponse.Message
                        };
                    }
                    else
                    {
                        return new AuthResponse
                        {
                            IsSuccess = false,
                            Message = "Login failed with unknown error."
                        };
                    }
                }


                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (authResponse != null && authResponse.IsSuccess && !string.IsNullOrWhiteSpace(authResponse.Token))
                {
                    // Store token in local storage
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                }

                return authResponse ?? new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Empty response from server"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string userId, string token)
        {
            return await _httpClient.GetFromJsonAsync<AuthResponse>($"api/account/confirm-email?userId={userId}&token={token}");
        }

        public async Task<AuthResponse> ForgotPasswordAsync(string email)
        {
            var model = new ForgotPasswordViewModel { Email = email };
            var response = await _httpClient.PostAsJsonAsync("api/account/forgot-password", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<AuthResponse>();
                return error ?? new AuthResponse { IsSuccess = false, Message = "Request failed." };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return result;
        }


        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/reset-password", model);
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<AuthResponse> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/change-password", model);
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<ProfileResponse> GetProfileAsync()
        {
            try
            {
                await AddJwtTokenAsync();

                var token = await _localStorage.GetItemAsStringAsync("jwt_token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new ProfileResponse
                    {
                        IsSuccess = false,
                        Message = "No authentication token found"
                    };
                }

                var response = await _httpClient.GetAsync("api/account/profile");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Profile error response: {errorContent}");

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await _localStorage.RemoveItemAsync("jwt_token");
                        return new ProfileResponse
                        {
                            IsSuccess = false,
                            Message = "Session expired. Please login again."
                        };
                    }

                    return new ProfileResponse
                    {
                        IsSuccess = false,
                        Message = $"Error: {response.StatusCode} - {errorContent}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ProfileResponse>();

                if (result != null && result.Profile == null)
                {
                    result.Profile = new UpdateProfileViewModel();
                }

                return result ?? new ProfileResponse
                {
                    IsSuccess = false,
                    Message = "Empty response from server"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile exception: {ex}");
                return new ProfileResponse
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }


        public async Task<AuthResponse> UpdateProfileAsync(UpdateProfileViewModel model)
        {
            await AddJwtTokenAsync();
            var response = await _httpClient.PutAsJsonAsync("api/account/profile", model);
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

    }
}