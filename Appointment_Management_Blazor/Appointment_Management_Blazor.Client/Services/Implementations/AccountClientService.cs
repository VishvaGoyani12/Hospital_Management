using Appointment_Management_Blazor.Client.Services.Interfaces;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Blazored.LocalStorage;
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

        public async Task<AuthResponse> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Add logging for the outgoing request
                Console.WriteLine($"Sending registration request for: {model.Email}");
                var modelJson = JsonSerializer.Serialize(model);
                Console.WriteLine($"Request payload: {modelJson}");

                var response = await _httpClient.PostAsJsonAsync("api/account/register", model);

                if (!response.IsSuccessStatusCode)
                {
                    // Read content as string first
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response content: {content}");

                    // Handle different content types
                    if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
                    {
                        try
                        {
                            // Try to parse as ProblemDetails
                            using var jsonDoc = JsonDocument.Parse(content);
                            var problemDetails = jsonDoc.RootElement;

                            if (problemDetails.TryGetProperty("title", out var title) ||
                                problemDetails.TryGetProperty("detail", out var detail))
                            {
                                return new AuthResponse
                                {
                                    IsSuccess = false,
                                };
                            }
                        }
                        catch (JsonException)
                        {
                            // Fall through to return raw content
                        }
                    }

                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = content ?? $"Registration failed with status: {response.StatusCode}"
                    };
                }

                return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Received empty response from server"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex}");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"Registration error: {ex.Message}"
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
                    var content = await response.Content.ReadAsStringAsync();
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = content
                    };
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
    }
}