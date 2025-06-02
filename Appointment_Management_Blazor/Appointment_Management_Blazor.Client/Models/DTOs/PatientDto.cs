namespace Appointment_Management_Blazor.Client.Models.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string JoinDate { get; set; }
        public bool Status { get; set; }
        public string StatusDisplay => Status ? "Active" : "Inactive";
    }

    public class PatientListResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<PatientDto> Data { get; set; } = new List<PatientDto>();
    }

}