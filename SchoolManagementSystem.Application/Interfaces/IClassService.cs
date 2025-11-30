using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IClassService
    {
        Task<ClassDto> GetClassByIdAsync(int id);
        Task<ClassDetailDto> GetClassDetailAsync(int id);
        Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto);
        Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto);
        Task<bool> DeleteClassAsync(int id);
        //Task<IEnumerable<EnrollmentDto>> GetClassEnrollmentsAsync(int classId);
        Task<bool> AssignTeacherToClassAsync(int classId, int teacherId);
        Task<bool> RemoveTeacherFromClassAsync(int classId);
        Task<APIResponseDto<ClassDto>> GetAllClassesAsync(SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<EnrollmentDto>> GetClassEnrollmentsAsync(int classId, SearchRequestDto request, string baseUrl);  

    }
}