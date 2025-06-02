// Client/Helper/AuthHeaderHandler.cs
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Appointment_Management_Blazor.Client.Helper
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

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
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    // Ensure the token is properly formatted (remove quotes if present)
                    token = token.Trim('"');
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch
            {
                // If token retrieval fails, continue without it
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}