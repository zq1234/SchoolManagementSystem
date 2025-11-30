using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IGradeService
    {
        Task<GradeDto> GetGradeByIdAsync(int id);
        Task<GradeDto> CreateGradeAsync(CreateGradeDto createGradeDto);
        Task<GradeDto> UpdateGradeAsync(int id, UpdateGradeDto updateGradeDto);
        Task<bool> DeleteGradeAsync(int id);
        Task<StudentCourseGradeDto> GetStudentCourseGradesAsync(int studentId, int courseId);
        Task<bool> BulkCreateGradesAsync(BulkCreateGradesDto bulkCreateGradesDto);
        Task<APIResponseDto<GradeDto>> GetAllGradesAsync(SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<GradeDto>> GetGradesByStudentAsync(int studentId, SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<GradeDto>> GetGradesByCourseAsync(int courseId, SearchRequestDto request, string baseUrl);  
    }
}