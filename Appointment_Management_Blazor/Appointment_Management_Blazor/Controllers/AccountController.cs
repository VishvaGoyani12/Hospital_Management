using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            // Log incoming model
            _logger.LogInformation("Received registration model: {@Model}", model);

            if (model == null)
            {
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Request body cannot be null"
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }


            var result = await _accountService.RegisterAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
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
