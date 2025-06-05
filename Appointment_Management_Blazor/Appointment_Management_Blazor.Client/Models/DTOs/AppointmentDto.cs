using System.ComponentModel.DataAnnotations;

namespace Appointment_Management_Blazor.Client.Models.DTOs
{
    public class AppointmentListResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<AppointmentDto> Data { get; set; } = new List<AppointmentDto>();
        public string? Error { get; set; }
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        [Required(ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentDateString => AppointmentDate.ToString("yyyy-MM-dd");
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class DoctorDropdownDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string SpecialistIn { get; set; } 
        public bool Status { get; set; } // Add if needed
    }

    public class PatientInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }


}
