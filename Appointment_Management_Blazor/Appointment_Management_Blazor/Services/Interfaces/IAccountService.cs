using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResponse> RegisterAsync(RegisterViewModel model);
        Task<AuthResponse> LoginAsync(LoginViewModel model);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
    }
}
