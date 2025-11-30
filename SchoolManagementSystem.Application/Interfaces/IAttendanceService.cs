using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> GetAttendanceByIdAsync(int id);
        Task<AttendanceDto> CreateAttendanceAsync(CreateAttendanceDto createAttendanceDto);
        Task<AttendanceDto> UpdateAttendanceAsync(int id, UpdateAttendanceDto updateAttendanceDto);
        Task<bool> DeleteAttendanceAsync(int id);
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int studentId, int courseId);
        Task<bool> BulkCreateAttendanceAsync(BulkCreateAttendanceDto bulkCreateAttendanceDto);
        Task<AttendanceReportDto> GenerateAttendanceReportAsync(int classId, DateTime startDate, DateTime endDate);
        Task<APIResponseDto<AttendanceDto>> GetAllAttendanceAsync(SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<AttendanceDto>> GetAttendanceByStudentAsync(int studentId, SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<AttendanceDto>> GetAttendanceByClassAsync(int classId, SearchRequestDto request, string baseUrl);  
        Task<APIResponseDto<AttendanceDto>> GetAttendanceByDateAsync(DateTime date, SearchRequestDto request, string baseUrl);  

    }
}