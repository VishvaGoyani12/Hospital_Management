using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

namespace Appointment_Management_Blazor.Client.Helper
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigation;
        private const string TokenKey = "jwt_token";

        public AuthHeaderHandler(ILocalStorageService localStorage, NavigationManager navigation)
        {
            _localStorage = localStorage;
            _navigation = navigation;
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

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Redirect to Access Denied page
                _navigation.NavigateTo("/access-denied");
            }

            return response;
        }
    }
}
