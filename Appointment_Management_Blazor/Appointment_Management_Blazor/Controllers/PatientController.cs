using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Repository.Implementations;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Appointment_Management_Blazor.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPatientStats()
        {
            var result = await _patientService.GetPatientStatsAsync();
            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] PatientFilterModel filter)
        {
            var result = await _patientService.GetAllPatientsAsync(filter);
            return Ok(result);
        }


        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit([FromBody] PatientViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _patientService.UpdatePatientAsync(model);
            if (!success) return NotFound();

            return Ok(new { success = true });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _patientService.DeletePatientAsync(id);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

    }
}
