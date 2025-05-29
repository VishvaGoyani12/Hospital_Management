namespace Appointment_Management_Blazor.Shared.Models
{
    public class DoctorFilterModel
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public string? SearchValue { get; set; }
        public string? Gender { get; set; }
        public string? Status { get; set; }
        public string? SpecialistIn { get; set; }
    }
}
