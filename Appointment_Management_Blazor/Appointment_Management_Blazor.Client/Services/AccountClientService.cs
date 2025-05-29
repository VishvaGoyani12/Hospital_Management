using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using System.Net.Http.Json;

namespace Appointment_Management_Blazor.Client.Services
{
    public class AccountClientService : IAccountClientService
    {
        private readonly HttpClient _httpClient;

        public AccountClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
    }
}
