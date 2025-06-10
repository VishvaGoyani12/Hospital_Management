

using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace Appointment_Management_Blazor.Client.Services.Interfaces
{
    public interface IAccountClientService
    {
        Task<AuthResponse> RegisterAsync(ClientRegisterModel model);
        Task<AuthResponse> LoginAsync(LoginViewModel model);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResponse> ChangePasswordAsync(ChangePasswordViewModel model);
        Task<ProfileResponse> GetProfileAsync();
        Task<AuthResponse> UpdateProfileAsync(UpdateProfileViewModel model);
        Task<ProfileResponse> UploadProfileImageAsync(IBrowserFile file);
        Task<AuthResponse> RemoveProfileImageAsync();
        Task<AuthResponse> InitiateEmailChangeAsync(string newEmail);
        Task<AuthResponse> ConfirmEmailChangeAsync(string userId, string newEmail, string token);
    }
}
