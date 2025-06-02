// PatientAppointmentController.cs
using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Appointment_Management_Blazor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientAppointmentController : ControllerBase
    {
        private readonly IPatientAppointmentService _appointmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientAppointmentController(IPatientAppointmentService appointmentService, UserManager<ApplicationUser> userManager)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetAll([FromBody] AppointmentFilterModel filters)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return Unauthorized();

                filters.PatientId = await _appointmentService.GetPatientIdByUserId(user.Id);
                var (total, data) = await _appointmentService.GetAllAppointmentsAsync(filters);

                return Ok(new
                {
                    draw = filters.Draw,
                    recordsTotal = total,
                    recordsFiltered = total,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound();
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /*
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentViewModel model)
        {
            try
            {
                var result = await _appointmentService.CreateAppointmentAsync(model);
                if (!result.Success)
                    return BadRequest(result.Message);
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AppointmentViewModel model)
        {
            try
            {
                var result = await _appointmentService.UpdateAppointmentAsync(model);
                if (!result.Success)
                    return BadRequest(result.Message);
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (!result.Success)
                    return BadRequest(result.Message);
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateTime? date, [FromQuery] int? selectedDoctorId)
        {
            try
            {
                if (date.HasValue && date.Value == DateTime.MinValue)
                {
                    return BadRequest("Invalid date parameter");
                }

                var doctors = await _appointmentService.GetAvailableDoctorsAsync(date, selectedDoctorId);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        */
    }
}