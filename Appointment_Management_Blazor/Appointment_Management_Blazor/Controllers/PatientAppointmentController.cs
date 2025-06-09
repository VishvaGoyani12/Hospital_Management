
using Appointment_Management_Blazor.EntityFrameworkCore.Data;
using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Appointment_Management_Blazor.Controllers
{
    [Authorize(Roles = "Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientAppointmentController : ControllerBase
    {
        private readonly IPatientAppointmentService _appointmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PatientAppointmentController(IPatientAppointmentService appointmentService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return Unauthorized();

                var patientId = await _appointmentService.GetPatientIdByUserId(user.Id);
                var stats = await _appointmentService.GetAppointmentStatsAsync(patientId);

                return Ok(new ApiResponse<AppointmentStatsDto>
                {
                    Success = true,
                    Data = stats
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


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentViewModel model)
        {
            try
            {
                var result = await _appointmentService.CreateAppointmentAsync(model);
                if (!result.Success)
                    return BadRequest(new ApiResponse { Success = false, Message = result.Message });

                return Ok(new ApiResponse { Success = true, Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AppointmentViewModel model)
        {
            try
            {
                var result = await _appointmentService.UpdateAppointmentAsync(model);
                if (!result.Success)
                    return BadRequest(new { Success = false, Message = result.Message });

                return Ok(new { Success = true, Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (!result.Success)
                    return BadRequest(new { Success = false, Message = result.Message });

                return Ok(new { Success = true, Message = result.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
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

        [HttpGet("current-patient")]
        public async Task<IActionResult> GetCurrentPatientInfo()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return Unauthorized();

                var patientId = await _appointmentService.GetPatientIdByUserId(user.Id);
                var patient = await _context.Patients
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.Id == patientId);

                if (patient == null) return NotFound();

                return Ok(new PatientInfoDto
                {
                    Id = patient.Id,
                    FullName = patient.ApplicationUser.FullName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}