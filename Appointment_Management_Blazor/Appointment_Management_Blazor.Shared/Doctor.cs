using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        public string SpecialistIn { get; set; }
        public bool Status { get; set; }

        public string? ProfileImagePath { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
