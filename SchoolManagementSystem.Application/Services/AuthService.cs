using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.DTOs.Authentication;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Register/Login/Refresh/Logout

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                    throw new BadRequestException("User with this email already exists.");

                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                    throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

                var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    throw new BadRequestException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                if (registerDto.Role == UserRoles.Student)
                    await CreateStudentProfile(user, registerDto);
                else if (registerDto.Role == UserRoles.Teacher)
                    await CreateTeacherProfile(user, registerDto);

                await _unitOfWork.CompleteAsync();

                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.CreateToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                await _emailService.SendWelcomeEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}");

                _logger.LogInformation("User registered successfully: {Email}", user.Email);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Roles = roles,
                    StudentId = registerDto.StudentId,
                    EmployeeId = registerDto.EmployeeId
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Invalid login attempt.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedException("Invalid login attempt.");

            user.UpdatedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            string? studentId = null;
            string? employeeId = null;

            if (roles.Contains(UserRoles.Student))
                studentId = (await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == user.Id))?.StudentId;
            else if (roles.Contains(UserRoles.Teacher))
                employeeId = (await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id))?.EmployeeId;

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            return new AuthResponseDto
            {

                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Email = user.Email!,
                UserName = user.UserName!,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles,
                StudentId = studentId,
                EmployeeId = employeeId
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedException("Invalid token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new UnauthorizedException("Invalid refresh token.");

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _tokenService.CreateToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Email = user.Email!,
                UserName = user.UserName!,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles
            };
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out: {UserId}", userId);
        }

        #endregion

        #region Change Password / Reset / Forgot

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
                throw new BadRequestException("New passwords do not match.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Password changed successfully for user: {Email}", user.Email);
            return true;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null || !user.IsActive) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email!, token);

            _logger.LogInformation("Password reset token sent to: {Email}", user.Email);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmNewPassword)
                throw new BadRequestException("Passwords do not match.");

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null || !user.IsActive)
                throw new BadRequestException("Invalid request.");

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);
            return true;
        }

        #endregion

        #region Paged Students / Teachers - CORRECTED TO MATCH INTERFACE

        public async Task<APIResponseDto<StudentDto>> GetAllStudentsAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _unitOfWork.Students.Query().Include(s => s.User).Where(s => s.User.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(s =>
                    s.User.FirstName.Contains(request.Search) ||
                    s.User.LastName.Contains(request.Search) ||
                    s.StudentId.Contains(request.Search));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.User.LastName)
                .ThenBy(s => s.User.FirstName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var studentDtos = _mapper.Map<List<StudentDto>>(students);

            return new APIResponseDto<StudentDto>(
                data: studentDtos,
                page: request.Page,
                pageSize: request.PageSize,
                totalCount: totalCount,
                baseUrl: baseUrl
            );
        }

        public async Task<APIResponseDto<TeacherDto>> GetAllTeachersAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _unitOfWork.Teachers.Query().Include(t => t.User).Where(t => t.User.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(t =>
                    t.User.FirstName.Contains(request.Search) ||
                    t.User.LastName.Contains(request.Search) ||
                    t.EmployeeId.Contains(request.Search) ||
                    t.Department.Contains(request.Search));
            }

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.User.LastName)
                .ThenBy(t => t.User.FirstName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var teacherDtos = _mapper.Map<List<TeacherDto>>(teachers);

            return new APIResponseDto<TeacherDto>(
                data: teacherDtos,
                page: request.Page,
                pageSize: request.PageSize,
                totalCount: totalCount,
                baseUrl: baseUrl
            );
        }

        #endregion

        #region Helpers

        private async Task CreateStudentProfile(User user, RegisterDto registerDto)
        {
            var student = new Student
            {
                UserId = user.Id,
                StudentId = registerDto.StudentId ?? GenerateStudentId(),
                EnrollmentDate = DateTime.UtcNow,
                DateOfBirth = registerDto.DateOfBirth ?? DateTime.UtcNow.AddYears(-18),
                Address = registerDto.Address ?? string.Empty,
                PhoneNumber = registerDto.PhoneNumber ?? string.Empty
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        private async Task CreateTeacherProfile(User user, RegisterDto registerDto)
        {
            var teacher = new Teacher
            {
                UserId = user.Id,
                EmployeeId = registerDto.EmployeeId ?? GenerateEmployeeId(),
                Department = registerDto.Department ?? "General",
                Qualification = registerDto.Qualification ?? string.Empty,
                HireDate = DateTime.UtcNow,
                Salary = registerDto.Salary ?? 0
            };

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.CompleteAsync();
        }

        private static string GenerateStudentId() => $"STU{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        private static string GenerateEmployeeId() => $"TCH{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        #endregion
    }
}