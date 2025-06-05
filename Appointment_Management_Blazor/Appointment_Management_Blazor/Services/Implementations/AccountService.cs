using Appointment_Management_Blazor.Data;
using Appointment_Management_Blazor.Services.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.HelperModel;
using Appointment_Management_Blazor.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Appointment_Management_Blazor.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return new AuthResponse { IsSuccess = false, Message = "Email already exists." };

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Gender = model.Gender,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new AuthResponse { IsSuccess = false, Message = errors };
                }

                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    ApplicationUserId = user.Id,
                    JoinDate = model.JoinDate,
                    Status = true
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationUrl = $"{_configuration["ClientUrl"]}/api/account/confirm-email?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";
                
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
$@"Please confirm your email by clicking the link below:<br/><br/>
<a href='{confirmationUrl}'>Click to confirm your email</a><br/><br/>
If the above link doesn't work, copy and paste this into your browser:<br/>
{confirmationUrl}");


                return new AuthResponse { IsSuccess = true, Message = "Registration successful! Please check your email to confirm your account." };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthResponse { IsSuccess = false, Message = "Invalid login attempt." };

            if (!user.EmailConfirmed)
                return new AuthResponse { IsSuccess = false, Message = "Email not confirmed." };

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
            if (patient != null && !patient.Status)
                return new AuthResponse { IsSuccess = false, Message = "Patient inactive." };

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
            if (doctor != null && !doctor.Status)
                return new AuthResponse { IsSuccess = false, Message = "Doctor inactive." };

            var token = await GenerateJwtToken(user);
            return new AuthResponse { IsSuccess = true, Token = token };
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new AuthResponse { IsSuccess = false, Message = "User not found." };

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return new AuthResponse
            {
                IsSuccess = result.Succeeded,
                Message = result.Succeeded ? "Email confirmed." : "Email confirmation failed."
            };
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),                  
        new Claim(ClaimTypes.NameIdentifier, user.Id),                    
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Email, user.Email)
    };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                authClaims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponse> ForgotPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new AuthResponse { IsSuccess = false, Message = "Email is required." };
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "No user found with this email." };
                }

                if (!user.EmailConfirmed)
                {
                    return new AuthResponse { IsSuccess = false, Message = "Email is not confirmed." };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = $"{_configuration["ClientUrl"]}/reset-password?token={HttpUtility.UrlEncode(token)}&email={HttpUtility.UrlEncode(email)}";

                await _emailSender.SendEmailAsync(email, "Reset Password",
    $@"Please reset your password by clicking the link below:<br/><br/>
    <a href='{resetUrl}'>Click to reset your password</a><br/><br/>
    If the above link doesn't work, copy and paste this into your browser:<br/>
    {resetUrl.Replace("&", "&amp;")}");

                return new AuthResponse { IsSuccess = true, Message = "Password reset link has been sent to your email." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "Invalid request." };
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "User not found." };
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return new AuthResponse { IsSuccess = true, Message = "Password has been reset successfully." };
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse { IsSuccess = false, Message = errors };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "Invalid request." };
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "User not found." };
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return new AuthResponse { IsSuccess = true, Message = "Password has been changed successfully." };
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse { IsSuccess = false, Message = errors };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePasswordAsync");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ProfileResponse> GetProfileAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"Looking for user with ID: {userId}");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found with ID: {userId}");
                    return new ProfileResponse
                    {
                        IsSuccess = false,
                        Message = $"User with ID {userId} not found in database"
                    };
                }

                _logger.LogInformation($"Found user: {user.Email}");

                var profile = new UpdateProfileViewModel
                {
                    FullName = user.FullName,
                    Gender = user.Gender,
                    Email = user.Email
                };

                return new ProfileResponse
                {
                    IsSuccess = true,
                    Message = "Profile retrieved successfully.",
                    Profile = profile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetProfileAsync for user ID: {userId}");
                return new ProfileResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponse { IsSuccess = false, Message = "User not found." };
                }

                user.FullName = model.FullName;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return new AuthResponse { IsSuccess = true, Message = "Profile updated successfully." };
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse { IsSuccess = false, Message = errors };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProfileAsync");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


    }


}
