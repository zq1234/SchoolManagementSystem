using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingStudentService : IStudentService
    {
        private readonly IStudentService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingStudentService> _logger;

        private static readonly TimeSpan StudentListCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan StudentDetailCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan StudentDashboardCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan StudentClassesCacheExpiration = TimeSpan.FromMinutes(10);

        public CachingStudentService(
            IStudentService decoratedService,
            ICacheService cacheService,
            ILogger<CachingStudentService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            var cacheKey = $"student_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentByIdAsync(id),
                StudentDetailCacheExpiration);
        }

        public async Task<StudentDetailDto> GetStudentDetailAsync(int id)
        {
            var cacheKey = $"student_detail_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentDetailAsync(id),
                StudentDetailCacheExpiration);
        }

        public async Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId)
        {
            var cacheKey = $"student_dashboard_{studentId}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentDashboardAsync(studentId),
                StudentDashboardCacheExpiration);
        }

        public async Task<APIResponseDto<StudentDto>> GetPagedStudentsAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"students_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetPagedStudentsAsync(request, baseUrl),
                StudentListCacheExpiration);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentEnrollmentsAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        public async Task<APIResponseDto<GradeDto>> GetStudentGradesAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_grades_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentGradesAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<StudentStatsDto> GetStudentStatsAsync(int studentId)
        {
            var cacheKey = $"student_stats_{studentId}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentStatsAsync(studentId),
                TimeSpan.FromMinutes(5));
        }

        public async Task<APIResponseDto<ClassDto>> GetStudentClassesAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_classes_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentClassesAsync(studentId, request, baseUrl),
                StudentClassesCacheExpiration);
        }

        // Write operations - invalidate cache
        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
        {
            var result = await _decoratedService.CreateStudentAsync(dto);

            await InvalidateStudentListCache();
            _logger.LogInformation("Invalidated students list cache after creating new student");

            return result;
        }

        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            var result = await _decoratedService.UpdateStudentAsync(id, dto);

            await InvalidateStudentCache(id);
            _logger.LogInformation("Invalidated student {StudentId} cache after update", id);

            return result;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var result = await _decoratedService.DeleteStudentAsync(id);

            if (result)
            {
                await InvalidateStudentCache(id);
                _logger.LogInformation("Invalidated all student {StudentId} cache after deletion", id);
            }

            return result;
        }

        // File operations
        public async Task<bool> UploadStudentPhotoAsync(StudentPhotoDto dto)
        {
            var result = await _decoratedService.UploadStudentPhotoAsync(dto);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"student_{dto.StudentId}"),
                    _cacheService.RemoveAsync($"student_detail_{dto.StudentId}")
                );
                _logger.LogDebug("Invalidated student {StudentId} cache after photo upload", dto.StudentId);
            }

            return result;
        }

        // Attendance and assignments
        public async Task<APIResponseDto<AttendanceDto>> GetStudentAttendanceAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_attendance_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentAttendanceAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<APIResponseDto<AssignmentDto>> GetStudentAssignmentsAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_assignments_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentAssignmentsAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        //public async Task<bool> SubmitAssignmentAsync(SubmitAssignmentDto dto)
        //{
        //    var result = await _decoratedService.SubmitAssignmentAsync(dto);

        //    if (result)
        //    {
        //        await _cacheService.RemoveByPatternAsync($"student_{dto.StudentId}_assignments*");
        //        _logger.LogDebug("Invalidated student {StudentId} assignments cache after submission", dto.StudentId);
        //    }

        //    return result;
        //}

        public async Task<APIResponseDto<NotificationDto>> GetStudentNotificationsAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            // Notifications change frequently, use shorter cache
            var cacheKey = $"student_{studentId}_notifications_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentNotificationsAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(2));
        }

        // Helper methods for cache invalidation
        private async Task InvalidateStudentCache(int studentId)
        {
            await Task.WhenAll(
                _cacheService.RemoveAsync($"student_{studentId}"),
                _cacheService.RemoveAsync($"student_detail_{studentId}"),
                _cacheService.RemoveAsync($"student_dashboard_{studentId}"),
                _cacheService.RemoveAsync($"student_stats_{studentId}"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_enrollments*"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_grades*"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_attendance*"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_assignments*"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_classes*"),
                _cacheService.RemoveByPatternAsync($"student_{studentId}_notifications*")
            );
        }

        private async Task InvalidateStudentListCache()
        {
            await _cacheService.RemoveByPatternAsync("students_list*");
        }
    }
}