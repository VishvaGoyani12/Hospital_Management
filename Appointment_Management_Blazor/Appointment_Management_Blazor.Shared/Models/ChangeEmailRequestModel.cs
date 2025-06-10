using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class ChangeEmailRequestModel
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }

    public class ConfirmEmailChangeModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string NewEmail { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
