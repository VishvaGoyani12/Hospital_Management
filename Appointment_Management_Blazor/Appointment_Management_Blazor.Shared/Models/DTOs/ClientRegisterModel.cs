using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.Models.DTOs
{
    public class ClientRegisterModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Today;
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public IBrowserFile? ProfileImage { get; set; }
    }
}
