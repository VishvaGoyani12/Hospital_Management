using Appointment_Management_Blazor.Client.Models.DTOs;
using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Appointment_Management_Blazor.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAppointmentController : ControllerBase
    {
        private readonly IDoctorAppointmentService _appointmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorAppointmentController(IDoctorAppointmentService appointmentService,
                                           UserManager<ApplicationUser> userManager)
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

                filters.DoctorId = await _appointmentService.GetDoctorIdByUserId(user.Id);
                var result = await _appointmentService.GetAllAppointmentsAsync(filters);

                return Ok(new
                {
                    draw = result.Draw,
                    recordsTotal = result.RecordsTotal,
                    recordsFiltered = result.RecordsFiltered,
                    data = result.Data,
                    error = result.Error
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
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

                return Ok(new ApiResponse<AppointmentViewModel>
                {
                    Success = true,
                    Data = appointment
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusModel model)
        {
            try
            {
                var (success, message) = await _appointmentService.UpdateAppointmentStatusAsync(model);
                if (!success)
                    return BadRequest(new ApiResponse { Success = false, Message = message });

                return Ok(new ApiResponse { Success = true, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("current-doctor")]
        public async Task<IActionResult> GetCurrentDoctorInfo()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return Unauthorized();

                var doctorId = await _appointmentService.GetDoctorIdByUserId(user.Id);
                var doctor = await _appointmentService.GetDoctorByIdAsync(doctorId);

                if (doctor == null) return NotFound();

                return Ok(new DoctorInfoDto
                {
                    Id = doctor.Id,
                    FullName = doctor.FullName,
                    SpecialistIn = doctor.SpecialistIn
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
