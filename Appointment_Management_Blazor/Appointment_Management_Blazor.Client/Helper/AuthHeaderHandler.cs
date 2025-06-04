// Client/Helper/AuthHeaderHandler.cs
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Appointment_Management_Blazor.Client.Helper
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "jwt_token";

        public AuthHeaderHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TokenKey);
                Console.WriteLine($"AuthHeaderHandler - Token: {token}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header set");
                }
                else
                {
                    Console.WriteLine("No token found in local storage");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting auth header: {ex.Message}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}