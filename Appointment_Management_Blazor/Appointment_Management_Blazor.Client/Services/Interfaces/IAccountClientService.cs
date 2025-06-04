using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IAccountClientService
    {
        Task<AuthResponse> RegisterAsync(RegisterViewModel model);
        Task<AuthResponse> LoginAsync(LoginViewModel model);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResponse> ChangePasswordAsync(ChangePasswordViewModel model);
        Task<ProfileResponse> GetProfileAsync();
        Task<AuthResponse> UpdateProfileAsync(UpdateProfileViewModel model);
    }
}
