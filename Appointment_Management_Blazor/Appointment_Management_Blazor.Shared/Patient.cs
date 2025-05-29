using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime? JoinDate { get; set; }
        public bool Status { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
