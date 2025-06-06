using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Appointment_Management_Blazor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger, IConfiguration configuration)
        {
            _accountService = accountService;
            _logger = logger;
            _configuration = configuration;
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
            _logger.LogInformation("ConfirmEmail called with userId: {UserId}", userId);

            try
            {
                var result = await _accountService.ConfirmEmailAsync(userId, token);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Email confirmation failed: {Message}", result.Message);
                    return Redirect($"{_configuration["ClientUrl"]}/login?error={Uri.EscapeDataString(result.Message)}");
                }

                _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
                return Redirect($"{_configuration["ClientUrl"]}/login?confirmed=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
                return Redirect($"{_configuration["ClientUrl"]}/login?error=An error occurred during confirmation");
            }
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = "Request body cannot be null" });
                }

                var result = await _accountService.ForgotPasswordAsync(model.Email);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = "Request body cannot be null" });
                }

                var result = await _accountService.ResetPasswordAsync(model);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = "Request body cannot be null" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponse { IsSuccess = false, Message = "User not authenticated" });
                }

                var result = await _accountService.ChangePasswordAsync(userId, model);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [Authorize(Roles = "Admin,Patient,Doctor")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID claim not found in token");
                    return Unauthorized(new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "User not authenticated - missing claims"
                    });
                }

                _logger.LogInformation($"Fetching profile for user ID: {userId}");

                var result = await _accountService.GetProfileAsync(userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"Profile not found for user ID: {userId}");
                    return NotFound(new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result.Message ?? "User profile not found"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching profile");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [Authorize(Roles = "Admin,Patient,Doctor")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = "Request body cannot be null" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponse { IsSuccess = false, Message = "User not authenticated" });
                }

                var result = await _accountService.UpdateProfileAsync(userId, model);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new AuthResponse
                {
                    IsSuccess = false,
                    Message = "An internal server error occurred"
                });
            }
        }
    }

}
