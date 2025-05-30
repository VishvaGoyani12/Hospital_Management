using System.Security.Claims;
using System.Text.Json;

namespace Appointment_Management_Blazor.Client.Helper
{
    public static class JwtParser
    {
        public static IEnumerable<string> GetRolesFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return Enumerable.Empty<string>();

            try
            {
                var parts = jwtToken.Split('.');
                if (parts.Length != 3)
                    return Enumerable.Empty<string>();

                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes.AsSpan());

                if (keyValuePairs == null)
                    return Enumerable.Empty<string>();

                if (keyValuePairs.TryGetValue("role", out var roles) ||
                    keyValuePairs.TryGetValue("roles", out roles) ||
                    keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out roles))
                {
                    if (roles.ValueKind == JsonValueKind.Array)
                    {
                        return roles.EnumerateArray()
                                  .Select(x => x.GetString())
                                  .Where(x => !string.IsNullOrEmpty(x))!;
                    }
                    else if (roles.ValueKind == JsonValueKind.String)
                    {
                        return new[] { roles.GetString()! };
                    }
                }

                if (keyValuePairs.TryGetValue("claims", out var claims) && claims.ValueKind == JsonValueKind.Array)
                {
                    return claims.EnumerateArray()
                               .Where(c => c.TryGetProperty("type", out var type) &&
                                          (type.GetString()?.EndsWith("/role") == true))
                               .Select(c => c.GetProperty("value").GetString())
                               .Where(x => !string.IsNullOrEmpty(x))!;
                }

                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        public static string? GetEmailFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return null;

            var parts = jwtToken.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes.AsSpan());

            if (keyValuePairs != null)
            {
                if (keyValuePairs.TryGetValue("email", out var email) ||
                    keyValuePairs.TryGetValue(ClaimTypes.Email, out email) ||
                    keyValuePairs.TryGetValue("sub", out email))
                {
                    if (email.ValueKind == JsonValueKind.String)
                    {
                        return email.GetString();
                    }
                }
            }

            return null;
        }

        public static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }

}