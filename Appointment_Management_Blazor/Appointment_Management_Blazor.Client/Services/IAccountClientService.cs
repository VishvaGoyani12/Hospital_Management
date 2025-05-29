using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Client.Services
{
    public interface IAccountClientService
    {
        Task<AuthResponse> RegisterAsync(RegisterViewModel model);
    }
}
