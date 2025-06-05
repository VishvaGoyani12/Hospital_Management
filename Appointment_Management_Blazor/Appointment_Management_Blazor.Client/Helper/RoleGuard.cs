using Blazored.LocalStorage;

namespace Appointment_Management_Blazor.Client.Helper
{
    public static class RoleGuard
    {
        public static async Task<bool> HasRoleAsync(ILocalStorageService localStorage, string role)
        {
            var token = await localStorage.GetItemAsync<string>("jwt_token");

            if (string.IsNullOrEmpty(token))
                return false;

            var roles = JwtParser.GetRolesFromToken(token);
            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public static async Task<bool> HasAnyRoleAsync(ILocalStorageService localStorage, params string[] rolesToCheck)
        {
            var token = await localStorage.GetItemAsync<string>("jwt_token");

            if (string.IsNullOrEmpty(token))
                return false;

            var userRoles = JwtParser.GetRolesFromToken(token);

            return userRoles.Any(r => rolesToCheck.Contains(r, StringComparer.OrdinalIgnoreCase));
        }
    }
}
