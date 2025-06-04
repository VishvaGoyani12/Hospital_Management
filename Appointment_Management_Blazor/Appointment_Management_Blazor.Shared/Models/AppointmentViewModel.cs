using System.ComponentModel.DataAnnotations;

namespace Appointment_Management_Blazor.Shared.Models
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
    }
   
}
