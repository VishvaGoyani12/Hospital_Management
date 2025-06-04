using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class AppointmentFilterModel
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? SearchValue { get; set; }
        public string? Status { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
    }

    public class DoctorInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialistIn { get; set; } = string.Empty;
    }

    public class UpdateStatusModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
