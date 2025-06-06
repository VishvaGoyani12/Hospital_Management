using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Interfaces.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResponse> RegisterAsync(RegisterViewModel model, string? profileImagePath);
        Task<AuthResponse> LoginAsync(LoginViewModel model);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
        Task<ProfileResponse> GetProfileAsync(string userId);
        Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileViewModel model);
    }
}
