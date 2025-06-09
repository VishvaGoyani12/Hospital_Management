using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Interfaces.Interfaces;

namespace Appointment_Management_Blazor.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDoctorStats()
        {
            var result = await _doctorService.GetDoctorStatsAsync();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors([FromQuery] DoctorFilterModel filters)
        {
            var result = await _doctorService.GetAllDoctorsAsync(filters);
            return Ok(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetDoctorList([FromBody] DoctorFilterModel filters)
        {
            var result = await _doctorService.GetAllDoctorsAsync(filters);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDoctor([FromForm] DoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _doctorService.CreateDoctorAsync(model);
            if (!success)
                return BadRequest(message);

            return Ok(new { Success = true, Message = message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(string id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(new
            {
                id = doctor.ApplicationUserId,  // key name "id" to match client DTO
                fullName = doctor.FullName,
                gender = doctor.Gender,
                email = doctor.Email,
                specialistIn = doctor.SpecialistIn,
                status = doctor.Status
            });
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditDoctor([FromForm] DoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            var (success, message) = await _doctorService.UpdateDoctorAsync(model);
            if (!success)
                return BadRequest(message);

            return Ok(new { Success = true, Message = message });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var (success, message) = await _doctorService.DeleteDoctorAsync(id);
            return Ok(new { success, message });
        }

        [HttpGet("specialists")]
        public async Task<IActionResult> GetSpecialistList()
        {
            var list = await _doctorService.GetSpecialistListAsync();
            return Ok(list);
        }
    }
}