using Microsoft.AspNetCore.Components.Forms;

namespace Appointment_Management_Blazor.Shared.Models.DTOs
{
    public class DoctorDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string SpecialistIn { get; set; }
        public bool Status { get; set; }
        public string StatusDisplay => Status ? "Active" : "Inactive";
        public string ProfileImagePath { get; set; } 
    }

    public class DoctorCreateEditModel
    {
        public string? ApplicationUserId { get; set; } 

        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string SpecialistIn { get; set; }
        public bool Status { get; set; }

        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        public IBrowserFile? ProfileImage { get; set; } 

        public string? ProfileImagePath { get; set; } 
    }

    public class DataStatsDto
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
    }

    public class DoctorListResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<DoctorDto> Data { get; set; } = new List<DoctorDto>();
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}