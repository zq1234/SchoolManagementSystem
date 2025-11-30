using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface ICourseService
    {
        Task<CourseDto> GetCourseByIdAsync(int id);
        Task<CourseDetailDto> GetCourseDetailAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
        Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> AssignTeacherToCourseAsync(int courseId, int teacherId);
        Task<bool> RemoveTeacherFromCourseAsync(int courseId);
        Task<APIResponseDto<CourseDto>> GetAllCoursesAsync(SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId, SearchRequestDto request, string baseUrl);  

    }
}