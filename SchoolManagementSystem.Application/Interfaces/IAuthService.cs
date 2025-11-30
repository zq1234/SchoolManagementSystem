using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Core.DTOs.Authentication;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task LogoutAsync(string userId);
        Task<APIResponseDto<StudentDto>> GetAllStudentsAsync(SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<TeacherDto>> GetAllTeachersAsync(SearchRequestDto request, string baseUrl);
    }
}