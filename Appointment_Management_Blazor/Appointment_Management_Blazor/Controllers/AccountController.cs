using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Appointment_Management_Blazor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", model?.Email);

                if (model == null)
                {
                    _logger.LogWarning("Null model received");
                    return BadRequest(new { Message = "Request body cannot be null" });
                }

                // Manual validation
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(model);
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    _logger.LogWarning("Validation failed: {Errors}", validationResults);
                    return BadRequest(new
                    {
                        Message = "Validation failed",
                        Errors = validationResults.Select(v => v.ErrorMessage)
                    });
                }

                var result = await _accountService.RegisterAsync(model);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Registration successful for: {Email}", model.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _accountService.LoginAsync(model);
            if (!result.IsSuccess)
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _accountService.ConfirmEmailAsync(userId, token);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }

}
