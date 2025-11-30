using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IAssignmentService
    {
        Task<AssignmentDto> GetAssignmentByIdAsync(int id);
        Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentDto createAssignmentDto, int teacherId);
        Task<AssignmentDto> UpdateAssignmentAsync(int id, UpdateAssignmentDto updateAssignmentDto);
        Task<bool> DeleteAssignmentAsync(int id);
        Task<AssignmentDetailDto> GetAssignmentDetailAsync(int id);
        Task<APIResponseDto<AssignmentDto>> GetAllAssignmentsAsync(SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<AssignmentDto>> GetAssignmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl);
        Task<APIResponseDto<AssignmentDto>> GetAssignmentsByTeacherAsync(int teacherId, SearchRequestDto request, string baseUrl); 

    }
}

