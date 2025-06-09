using Appointment_Management_Blazor.EntityFrameworkCore.Data;
using Appointment_Management_Blazor.Interfaces.Interfaces;
using Appointment_Management_Blazor.Shared;
using Appointment_Management_Blazor.Shared.Models;
using Appointment_Management_Blazor.Shared.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Appointment_Management_Blazor.Repository.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;


        public DoctorService(ApplicationDbContext context,
                             UserManager<ApplicationUser> userManager,
                             RoleManager<IdentityRole> roleManager,IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }
        public async Task<DataStatsDto> GetDoctorStatsAsync()
        {
            var total = await _context.Doctors.CountAsync();
            var active = await _context.Doctors.CountAsync(d => d.Status);
            var inactive = await _context.Doctors.CountAsync(d => !d.Status);

            return new DataStatsDto
            {
                TotalData = total,
                ActiveData = active,
                InactiveData = inactive
            };
        }

        public async Task<DoctorListResponse> GetAllDoctorsAsync(DoctorFilterModel filters)
        {
            var query = _context.Doctors
                .Include(d => d.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filters.SearchValue))
            {
                query = query.Where(d =>
                    d.ApplicationUser.FullName.Contains(filters.SearchValue) ||
                    d.SpecialistIn.Contains(filters.SearchValue) ||
                    d.ApplicationUser.Email.Contains(filters.SearchValue));
            }

            if (filters.Status.HasValue)
            {
                query = query.Where(d => d.Status == filters.Status.Value);
            }

            if (!string.IsNullOrEmpty(filters.Gender))
            {
                query = query.Where(d => d.ApplicationUser.Gender == filters.Gender);
            }

            if (!string.IsNullOrEmpty(filters.SpecialistIn))
            {
                query = query.Where(d => d.SpecialistIn == filters.SpecialistIn);
            }

            var totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(filters.SortColumn))
            {
                switch (filters.SortColumn)
                {
                    case "FullName":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.FullName)
                            : query.OrderBy(d => d.ApplicationUser.FullName);
                        break;
                    case "Gender":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.Gender)
                            : query.OrderBy(d => d.ApplicationUser.Gender);
                        break;
                    case "Email":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.ApplicationUser.Email)
                            : query.OrderBy(d => d.ApplicationUser.Email);
                        break;
                    case "SpecialistIn":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.SpecialistIn)
                            : query.OrderBy(d => d.SpecialistIn);
                        break;
                    case "Status":
                        query = filters.SortDirection == "desc"
                            ? query.OrderByDescending(d => d.Status)
                            : query.OrderBy(d => d.Status);
                        break;
                    default:
                        query = query.OrderBy(d => d.ApplicationUser.FullName);
                        break;
                }
            }


            var doctors = await query
        .Skip(filters.Start)
        .Take(filters.Length)
        .Select(d => new DoctorDto
        {
            Id = d.ApplicationUserId,
            FullName = d.ApplicationUser.FullName,
            Gender = d.ApplicationUser.Gender,
            Email = d.ApplicationUser.Email,
            SpecialistIn = d.SpecialistIn,
            Status = d.Status,
            ProfileImagePath = d.ProfileImagePath 
        })
        .ToListAsync();

            return new DoctorListResponse
            {
                Draw = filters.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = doctors
            };
        }


        public async Task<(bool Success, string Message)> CreateDoctorAsync(DoctorViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return (false, "Password and confirmation password do not match.");
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return (false, "A doctor with this email already exists.");

            // Handle image upload
            string imagePath = null;
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                imagePath = await SaveProfileImage(model.ProfileImage, "doctors");
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Gender = model.Gender,
                Email = model.Email,
                UserName = model.Email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, string.Join("<br/>", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("Doctor"))
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                ApplicationUserId = user.Id,
                SpecialistIn = model.SpecialistIn,
                Status = model.Status,
                ProfileImagePath = imagePath
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return (true, "Doctor created successfully");
        }

        public async Task<DoctorViewModel?> GetDoctorByIdAsync(string id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

            if (doctor == null || doctor.ApplicationUser == null)
                return null;

            return new DoctorViewModel
            {
                ApplicationUserId = doctor.ApplicationUserId,
                FullName = doctor.ApplicationUser.FullName,
                Gender = doctor.ApplicationUser.Gender,
                Email = doctor.ApplicationUser.Email,
                SpecialistIn = doctor.SpecialistIn,
                Status = doctor.Status
            };
        }

        public async Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.ApplicationUserId))
                {
                    return (false, "User ID is required");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
                if (user == null)
                {
                    Console.WriteLine($"User not found with ID: {model.ApplicationUserId}");
                    return (false, $"User not found with ID: {model.ApplicationUserId}");
                }

                // Find doctor
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == model.ApplicationUserId);

                if (doctor == null)
                {
                    return (false, "Doctor not found");
                }
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(doctor.ProfileImagePath))
                    {
                        DeleteProfileImage(doctor.ProfileImagePath);
                    }

                    doctor.ProfileImagePath = await SaveProfileImage(model.ProfileImage, "doctors");
                }

                // Check if doctor has pending or confirmed appointments
                var hasAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctor.Id &&
                                  (a.Status == "Pending" || a.Status == "Confirmed"));

                // Prevent deactivation if has appointments
                if (hasAppointments && model.Status == false)
                {
                    return (false, "Cannot deactivate doctor with existing pending or confirmed appointments");
                }

                // Update user
                user.FullName = model.FullName;
                user.Gender = model.Gender;

                if (!string.IsNullOrEmpty(model.Email))
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.NormalizedEmail = model.Email.ToUpper();
                    user.NormalizedUserName = model.Email.ToUpper();
                }

                var userResult = await _userManager.UpdateAsync(user);
                if (!userResult.Succeeded)
                {
                    return (false, string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }

                // Update password
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (!passResult.Succeeded)
                    {
                        return (false, string.Join(", ", passResult.Errors.Select(e => e.Description)));
                    }
                }

                doctor.SpecialistIn = model.SpecialistIn;
                doctor.Status = model.Status;

                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();

                return (true, "Doctor updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating doctor: {ex}");
                return (false, $"Error updating doctor: {ex.Message}");
            }
        }

        private async Task<string> SaveProfileImage(IFormFile imageFile, string folderName)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{fileName}";
        }

        private void DeleteProfileImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }


        public async Task<(bool Success, string Message)> DeleteDoctorAsync(string id)
        {
            var doctor = await _context.Doctors.Include(d => d.ApplicationUser)
                                               .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

            if (doctor == null)
                return (false, "Doctor not found");

            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctor.Id &&
                              (a.Status == "Pending" || a.Status == "Confirmed"));

            if (hasAppointments)
                return (false, "Cannot delete doctor with existing pending or confirmed appointments");

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(doctor.ApplicationUser);
            if (!result.Succeeded)
                return (false, string.Join("<br/>", result.Errors.Select(e => e.Description)));

            return (true, "Doctor deleted successfully");
        }

        public async Task<List<string>> GetSpecialistListAsync()
        {
            return await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.SpecialistIn))
                .Select(d => d.SpecialistIn)
                .Distinct()
                .ToListAsync();
        }
    }
}
