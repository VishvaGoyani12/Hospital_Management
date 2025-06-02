using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class PatientFilterModel
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? SearchValue { get; set; }

        public string? Gender { get; set; }
        public bool? Status { get; set; }
        public DateTime? JoinDate { get; set; }
    }
}
