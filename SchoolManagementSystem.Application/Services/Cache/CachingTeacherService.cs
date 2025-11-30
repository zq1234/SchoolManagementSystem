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
    public class CachingTeacherService : ITeacherService
    {
        private readonly ITeacherService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingTeacherService> _logger;

        // Cache expiration times
        private static readonly TimeSpan TeacherListCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan TeacherDetailCacheExpiration = TimeSpan.FromMinutes(20);
        private static readonly TimeSpan TeacherStatsCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan TeacherCoursesCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan TeacherClassesCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ClassStudentsCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClassAttendanceCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan ClassAssignmentsCacheExpiration = TimeSpan.FromMinutes(10);

        public CachingTeacherService(
            ITeacherService decoratedService,
            ICacheService cacheService,
            ILogger<CachingTeacherService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TeacherDto> GetTeacherByIdAsync(int id)
        {
            var cacheKey = $"teacher_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetTeacherByIdAsync(id),
                TeacherDetailCacheExpiration);
        }

        public async Task<TeacherDetailDto> GetTeacherDetailAsync(int id)
        {
            var cacheKey = $"teacher_detail_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetTeacherDetailAsync(id),
                TeacherDetailCacheExpiration);
        }

        public async Task<TeacherStatsDto> GetTeacherStatsAsync(int teacherId)
        {
            var cacheKey = $"teacher_stats_{teacherId}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetTeacherStatsAsync(teacherId),
                TeacherStatsCacheExpiration);
        }

        public async Task<APIResponseDto<TeacherDto>> GetAllTeachersAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"teachers_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllTeachersAsync(request, baseUrl),
                TeacherListCacheExpiration);
        }

        public async Task<APIResponseDto<CourseDto>> GetTeacherCoursesAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"teacher_{teacherId}_courses_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetTeacherCoursesAsync(teacherId, request, baseUrl),
                TeacherCoursesCacheExpiration);
        }

        public async Task<APIResponseDto<ClassDto>> GetTeacherClassesAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"teacher_{teacherId}_classes_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetTeacherClassesAsync(teacherId, request, baseUrl),
                TeacherClassesCacheExpiration);
        }

        public async Task<APIResponseDto<StudentDto>> GetClassStudentsAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_students_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassStudentsAsync(classId, request, baseUrl),
                ClassStudentsCacheExpiration);
        }

        public async Task<APIResponseDto<AttendanceDto>> GetClassAttendanceHistoryAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_attendance_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassAttendanceHistoryAsync(classId, request, baseUrl),
                ClassAttendanceCacheExpiration);
        }

        public async Task<APIResponseDto<AssignmentDto>> GetClassAssignmentsAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_assignments_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassAssignmentsAsync(classId, request, baseUrl),
                ClassAssignmentsCacheExpiration);
        }

        // Class management methods
        public async Task<ClassDto> GetClassByIdAsync(int id)
        {
            var cacheKey = $"class_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassByIdAsync(id),
                TimeSpan.FromMinutes(15));
        }

        public async Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            var result = await _decoratedService.CreateClassAsync(createClassDto);

            await InvalidateTeacherClassCache((int)createClassDto.TeacherId);
            _logger.LogInformation("Invalidated teacher {TeacherId} class cache after creating new class", createClassDto.TeacherId);

            return result;
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto)
        {
            var result = await _decoratedService.UpdateClassAsync(id, updateClassDto);

            await _cacheService.RemoveAsync($"class_{id}");
            await _cacheService.RemoveByPatternAsync($"class_{id}_*");
            _logger.LogDebug("Invalidated class {ClassId} cache after update", id);

            return result;
        }

        public async Task<bool> DeactivateClassAsync(int id)
        {
            var result = await _decoratedService.DeactivateClassAsync(id);

            if (result)
            {
                await _cacheService.RemoveAsync($"class_{id}");
                await _cacheService.RemoveByPatternAsync($"class_{id}_*");
                _logger.LogDebug("Invalidated class {ClassId} cache after deactivation", id);
            }

            return result;
        }

        // Write operations - invalidate cache
        public async Task<TeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
        {
            var result = await _decoratedService.CreateTeacherAsync(createTeacherDto);

            await InvalidateTeacherListCache();
            _logger.LogInformation("Invalidated teachers list cache after creating new teacher");

            return result;
        }

        public async Task<TeacherDto> UpdateTeacherAsync(int id, UpdateTeacherDto updateTeacherDto)
        {
            var result = await _decoratedService.UpdateTeacherAsync(id, updateTeacherDto);

            await InvalidateTeacherCache(id);
            _logger.LogInformation("Invalidated teacher {TeacherId} cache after update", id);

            return result;
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var result = await _decoratedService.DeleteTeacherAsync(id);

            if (result)
            {
                await InvalidateTeacherCache(id);
                _logger.LogInformation("Invalidated all teacher {TeacherId} cache after deletion", id);
            }

            return result;
        }

        // Class enrollment methods
        public async Task<bool> EnrollStudentInClassAsync(int classId, int studentId, int teacherId)
        {
            var result = await _decoratedService.EnrollStudentInClassAsync(classId, studentId, teacherId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveByPatternAsync($"class_{classId}_students*"),
                    _cacheService.RemoveByPatternAsync($"student_{studentId}_classes*"),
                    _cacheService.RemoveByPatternAsync($"student_{studentId}_enrollments*")
                );
                _logger.LogDebug("Invalidated class {ClassId} and student {StudentId} cache after enrollment", classId, studentId);
            }

            return result;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int classId, int studentId)
        {
            var result = await _decoratedService.RemoveStudentFromClassAsync(classId, studentId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveByPatternAsync($"class_{classId}_students*"),
                    _cacheService.RemoveByPatternAsync($"student_{studentId}_classes*"),
                    _cacheService.RemoveByPatternAsync($"student_{studentId}_enrollments*")
                );
                _logger.LogDebug("Invalidated class {ClassId} and student {StudentId} cache after removal", classId, studentId);
            }

            return result;
        }

        public async Task<BulkEnrollmentResultDto> BulkEnrollStudentsAsync(int classId, List<int> studentIds, int teacherId)
        {
            var result = await _decoratedService.BulkEnrollStudentsAsync(classId, studentIds, teacherId);

            if (result.Successful > 0)
            {
                await Task.WhenAll(
                    _cacheService.RemoveByPatternAsync($"class_{classId}_students*"),
                    Task.WhenAll(studentIds.Select(studentId =>
                        _cacheService.RemoveByPatternAsync($"student_{studentId}_classes*")))
                );
                _logger.LogDebug("Invalidated class {ClassId} and student cache after bulk enrollment", classId);
            }

            return result;
        }

        // Helper methods for cache invalidation
        private async Task InvalidateTeacherCache(int teacherId)
        {
            await Task.WhenAll(
                _cacheService.RemoveAsync($"teacher_{teacherId}"),
                _cacheService.RemoveAsync($"teacher_detail_{teacherId}"),
                _cacheService.RemoveAsync($"teacher_stats_{teacherId}"),
                _cacheService.RemoveByPatternAsync($"teacher_{teacherId}_courses*"),
                _cacheService.RemoveByPatternAsync($"teacher_{teacherId}_classes*"),
                InvalidateTeacherListCache()
            );
        }

        private async Task InvalidateTeacherClassCache(int teacherId)
        {
            await Task.WhenAll(
                _cacheService.RemoveByPatternAsync($"teacher_{teacherId}_classes*"),
                _cacheService.RemoveByPatternAsync($"teacher_{teacherId}_courses*")
            );
        }

        private async Task InvalidateTeacherListCache()
        {
            await _cacheService.RemoveByPatternAsync("teachers_list*");
        }
    }
}