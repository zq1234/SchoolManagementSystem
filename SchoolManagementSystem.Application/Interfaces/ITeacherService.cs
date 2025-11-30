using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
namespace SchoolManagementSystem.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<TeacherDto> GetTeacherByIdAsync(int id);
        Task<TeacherDetailDto> GetTeacherDetailAsync(int id);
        Task<TeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto);
        Task<TeacherDto> UpdateTeacherAsync(int id, UpdateTeacherDto updateTeacherDto);
        Task<bool> DeleteTeacherAsync(int id);
        Task<TeacherStatsDto> GetTeacherStatsAsync(int teacherId);
        Task<APIResponseDto<TeacherDto>> GetAllTeachersAsync(SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<CourseDto>> GetTeacherCoursesAsync(int teacherId, SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<ClassDto>> GetTeacherClassesAsync(int teacherId, SearchRequestDto request, string baseUrl);
        Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
        Task<APIResponseDto<StudentDto>> GetClassStudentsAsync(int classId, SearchRequestDto request, string baseUrl);
        Task<bool> EnrollStudentInClassAsync(int classId, int studentId, int teacherId);
        Task<BulkEnrollmentResultDto> BulkEnrollStudentsAsync(int classId, List<int> studentIds, int teacherId);
        Task<ClassDto> GetClassByIdAsync(int id);
        Task<APIResponseDto<AssignmentDto>> GetClassAssignmentsAsync(int classId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<AttendanceDto>> GetClassAttendanceHistoryAsync(int classId, SearchRequestDto request, string baseUrl);
        Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto);
        Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto);
        Task<bool> DeactivateClassAsync(int id);

    }
}