using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface ISubmissionService
    {
        Task<SubmissionDto> GetSubmissionByIdAsync(int id);
        Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionDto createSubmissionDto, int studentId);
        Task<SubmissionDto> UpdateSubmissionAsync(int id, UpdateSubmissionDto updateSubmissionDto);
        Task<SubmissionDto> GradeSubmissionAsync(int id, GradeSubmissionDto gradeSubmissionDto, int teacherId);
        Task<bool> DeleteSubmissionAsync(int id);
        Task<SubmissionStatsDto> GetSubmissionStatsAsync(int assignmentId);
        Task<APIResponseDto<SubmissionDto>> GetSubmissionsByAssignmentAsync(int assignmentId, SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<SubmissionDto>> GetSubmissionsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl);  
    }
}
