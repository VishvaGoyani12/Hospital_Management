using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string SpecialistIn { get; set; }

        public IFormFile? ProfileImage { get; set; }  

        public string? ProfileImagePath { get; set; }

        public bool Status { get; set; }
    }
}
