using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class DoctorDropdownViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialistIn { get; set; } = string.Empty;
    }
}
