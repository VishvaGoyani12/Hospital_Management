namespace Appointment_Management_Blazor.Shared.Models.DTOs
{
    public class DoctorDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string SpecialistIn { get; set; }
        public bool Status { get; set; }
        public string StatusDisplay => Status ? "Active" : "Inactive";
    }

    public class DoctorListResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<DoctorDto> Data { get; set; } = new List<DoctorDto>();
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}