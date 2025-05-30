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

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _configuration = configuration;
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
                var confirmationUrl = $"{_configuration["BaseUrl"]}/api/account/confirm-email?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Click here to confirm: <a href='{confirmationUrl}'>Confirm</a>");

                return new AuthResponse { IsSuccess = true, Message = "Registration successful! Please check your email to confirm your account." };
            }
            catch (Exception ex)
            {
                // Log the exception here
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
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("email", user.Email),
        new Claim(ClaimTypes.Name, user.UserName)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }


}
