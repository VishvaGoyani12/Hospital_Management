using Appointment_Management_Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment_Management_Blazor.Shared.Models;

namespace Appointment_Management_Blazor.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorApiController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorApiController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors([FromQuery] DoctorFilterModel filters)
        {
            var result = await _doctorService.GetAllDoctorsAsync(filters);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorViewModel model)
        {
            var (success, message) = await _doctorService.CreateDoctorAsync(model);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(string id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditDoctor([FromBody] DoctorViewModel model)
        {
            var (success, message) = await _doctorService.UpdateDoctorAsync(model);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true });
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
