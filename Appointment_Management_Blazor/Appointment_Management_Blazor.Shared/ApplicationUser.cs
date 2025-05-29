using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
