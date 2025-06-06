namespace Appointment_Management_Blazor.Shared.Models
{
    public class PatientViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? JoinDate { get; set; }
        public bool Status { get; set; }
        public string ProfileImagePath { get; set; } // Add this property
    }
}
