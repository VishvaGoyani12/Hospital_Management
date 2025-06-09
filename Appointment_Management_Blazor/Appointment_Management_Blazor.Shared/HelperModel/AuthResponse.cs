using Appointment_Management_Blazor.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.HelperModel
{
    public class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }  
    }

    public class ProfileResponse : AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public UpdateProfileViewModel? Profile { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
