using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDto> GetStudentByIdAsync(int id);
        Task<StudentDetailDto> GetStudentDetailAsync(int id);
        Task<APIResponseDto<StudentDto>> GetPagedStudentsAsync(SearchRequestDto request, string baseUrl);
        Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto);
        Task<bool> DeleteStudentAsync(int id);
        Task<APIResponseDto<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<GradeDto>> GetStudentGradesAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<AttendanceDto>> GetStudentAttendanceAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<StudentStatsDto> GetStudentStatsAsync(int studentId);
        Task<bool> UploadStudentPhotoAsync(StudentPhotoDto dto);
        Task<APIResponseDto<AssignmentDto>> GetStudentAssignmentsAsync(int studentId, SearchRequestDto request, string baseUrl);
       // Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionDto createSubmissionDto, int studentId);
        // Task<bool> SubmitAssignmentAsync(SubmitAssignmentDto dto);
        Task<APIResponseDto<NotificationDto>> GetStudentNotificationsAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<ClassDto>> GetStudentClassesAsync(int studentId, SearchRequestDto request, string baseUrl);
        Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId);
    }
}