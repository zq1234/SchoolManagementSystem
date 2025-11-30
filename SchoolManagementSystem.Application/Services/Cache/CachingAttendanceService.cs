using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingAttendanceService : IAttendanceService
    {
        private readonly IAttendanceService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingAttendanceService> _logger;

        private static readonly TimeSpan AttendanceListCacheExpiration = TimeSpan.FromMinutes(5); // Attendance changes frequently
        private static readonly TimeSpan AttendanceDetailCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan AttendanceSummaryCacheExpiration = TimeSpan.FromMinutes(5);

        public CachingAttendanceService(
            IAttendanceService decoratedService,
            ICacheService cacheService,
            ILogger<CachingAttendanceService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AttendanceDto> GetAttendanceByIdAsync(int id)
        {
            var cacheKey = $"attendance_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAttendanceByIdAsync(id),
                AttendanceDetailCacheExpiration);
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAllAttendanceAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"attendance_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllAttendanceAsync(request, baseUrl),
                AttendanceListCacheExpiration);
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_attendance_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAttendanceByStudentAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByClassAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_attendance_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAttendanceByClassAsync(classId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByDateAsync(DateTime date, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"attendance_date_{date:yyyyMMdd}_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAttendanceByDateAsync(date, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int studentId, int courseId)
        {
            var cacheKey = $"student_{studentId}_course_{courseId}_attendance_summary";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAttendanceSummaryAsync(studentId, courseId),
                AttendanceSummaryCacheExpiration);
        }

        public async Task<AttendanceReportDto> GenerateAttendanceReportAsync(int classId, DateTime startDate, DateTime endDate)
        {
            var cacheKey = $"attendance_report_class_{classId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GenerateAttendanceReportAsync(classId, startDate, endDate),
                TimeSpan.FromMinutes(10)); // Reports can be cached longer
        }

        // Write operations
        public async Task<AttendanceDto> CreateAttendanceAsync(CreateAttendanceDto createAttendanceDto)
        {
            var result = await _decoratedService.CreateAttendanceAsync(createAttendanceDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync("attendance_list_"),
                _cacheService.RemoveAsync($"student_{createAttendanceDto.StudentId}_attendance"),
                _cacheService.RemoveAsync($"class_{createAttendanceDto.ClassId}_attendance"),
                _cacheService.RemoveAsync($"attendance_date_{createAttendanceDto.Date:yyyyMMdd}"),
                _cacheService.RemoveAsync($"student_{createAttendanceDto.StudentId}_course_*_attendance_summary") // Wildcard removal
            );

            _logger.LogInformation("Invalidated attendance caches after creating new attendance record");
            return result;
        }

        public async Task<AttendanceDto> UpdateAttendanceAsync(int id, UpdateAttendanceDto updateAttendanceDto)
        {
            // Get existing attendance to know what to invalidate
            var existingAttendance = await _decoratedService.GetAttendanceByIdAsync(id);

            var result = await _decoratedService.UpdateAttendanceAsync(id, updateAttendanceDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"attendance_{id}"),
                _cacheService.RemoveAsync("attendance_list_"),
                _cacheService.RemoveAsync($"student_{existingAttendance.StudentId}_attendance"),
                _cacheService.RemoveAsync($"class_{existingAttendance.ClassId}_attendance"),
                _cacheService.RemoveAsync($"attendance_date_{existingAttendance.Date:yyyyMMdd}"),
                _cacheService.RemoveAsync($"student_{existingAttendance.StudentId}_course_*_attendance_summary")
            );

            _logger.LogInformation("Invalidated attendance {AttendanceId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            // Get existing attendance to know what to invalidate
            var existingAttendance = await _decoratedService.GetAttendanceByIdAsync(id);

            var result = await _decoratedService.DeleteAttendanceAsync(id);

            if (result)
            {
                // Invalidate all attendance-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"attendance_{id}"),
                    _cacheService.RemoveAsync("attendance_list_"),
                    _cacheService.RemoveAsync($"student_{existingAttendance.StudentId}_attendance"),
                    _cacheService.RemoveAsync($"class_{existingAttendance.ClassId}_attendance"),
                    _cacheService.RemoveAsync($"attendance_date_{existingAttendance.Date:yyyyMMdd}"),
                    _cacheService.RemoveAsync($"student_{existingAttendance.StudentId}_course_*_attendance_summary")
                );

                _logger.LogInformation("Invalidated all attendance {AttendanceId} cache after deletion", id);
            }

            return result;
        }

        public async Task<bool> BulkCreateAttendanceAsync(BulkCreateAttendanceDto bulkCreateAttendanceDto)
        {
            var result = await _decoratedService.BulkCreateAttendanceAsync(bulkCreateAttendanceDto);

            if (result)
            {
                // Invalidate class attendance and date-specific caches
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"class_{bulkCreateAttendanceDto.ClassId}_attendance"),
                    _cacheService.RemoveAsync($"attendance_date_{bulkCreateAttendanceDto.Date:yyyyMMdd}"),
                    _cacheService.RemoveAsync("attendance_list_")
                );

                // Also invalidate student attendance summaries
                foreach (var attendanceItem in bulkCreateAttendanceDto.Attendances)
                {
                    await _cacheService.RemoveAsync($"student_{attendanceItem.StudentId}_attendance");
                    await _cacheService.RemoveAsync($"student_{attendanceItem.StudentId}_course_*_attendance_summary");
                }

                _logger.LogInformation("Invalidated attendance caches after bulk attendance creation for class {ClassId}", bulkCreateAttendanceDto.ClassId);
            }

            return result;
        }
    }
}