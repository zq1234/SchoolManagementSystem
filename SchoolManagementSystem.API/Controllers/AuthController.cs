using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.API.Extensions;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.DTOs.Authentication;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(new ApiResponse<AuthResponseDto>(result, "User registered successfully"));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(new ApiResponse<AuthResponseDto>(result, "Login successful"));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]

        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(new ApiResponse<AuthResponseDto>(result, "Token refreshed successfully"));
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.GetUserId();
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            return Ok(new ApiResponse<bool>(result, "Password changed successfully"));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(new ApiResponse<bool>(true, "Password reset email sent successfully"));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(new ApiResponse<bool>(result, "Password reset successfully"));
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            await _authService.LogoutAsync(userId);
            return Ok(new ApiResponse<bool>(true, "Logout successful"));
        }

        [HttpGet("students")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllStudents([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var students = await _authService.GetAllStudentsAsync(request, baseUrl);
            return Ok(students);
           // return Ok(new ApiResponse<APIResponseDto<StudentDto>>(students, "Students retrieved successfully"));
        }

        [HttpGet("teachers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeachers([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var teachers = await _authService.GetAllTeachersAsync(request, baseUrl);
            return Ok(teachers);
           // return Ok(new ApiResponse<APIResponseDto<TeacherDto>>(teachers, "Teachers retrieved successfully"));
        }

    }
}