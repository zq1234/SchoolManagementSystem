using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentDto> GetEnrollmentByIdAsync(int id);
        Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
        Task<EnrollmentDto> UpdateEnrollmentAsync(int id, UpdateEnrollmentDto updateEnrollmentDto);
        Task<bool> DeleteEnrollmentAsync(int id);
        Task<bool> UpdateEnrollmentStatusAsync(int id, EnrollmentStatusDto statusDto);
        Task<APIResponseDto<EnrollmentDto>> GetAllEnrollmentsAsync(SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl);

    }
}