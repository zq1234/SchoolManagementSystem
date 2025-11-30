using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingCourseService : ICourseService
    {
        private readonly ICourseService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingCourseService> _logger;

        private static readonly TimeSpan CourseListCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan CourseDetailCacheExpiration = TimeSpan.FromMinutes(20);

        public CachingCourseService(
            ICourseService decoratedService,
            ICacheService cacheService,
            ILogger<CachingCourseService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<CourseDto> GetCourseByIdAsync(int id)
        {
            var cacheKey = $"course_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetCourseByIdAsync(id),
                CourseDetailCacheExpiration);
        }

        public async Task<CourseDetailDto> GetCourseDetailAsync(int id)
        {
            var cacheKey = $"course_detail_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetCourseDetailAsync(id),
                CourseDetailCacheExpiration);
        }

        public async Task<APIResponseDto<CourseDto>> GetAllCoursesAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"courses_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllCoursesAsync(request, baseUrl),
                CourseListCacheExpiration);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"course_{courseId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetCourseEnrollmentsAsync(courseId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        // Write operations
        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
        {
            var result = await _decoratedService.CreateCourseAsync(createCourseDto);

            await _cacheService.RemoveAsync("courses_list_");
            _logger.LogInformation("Invalidated courses list cache after creating new course");

            return result;
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto)
        {
            var result = await _decoratedService.UpdateCourseAsync(id, updateCourseDto);

            await Task.WhenAll(
                _cacheService.RemoveAsync($"course_{id}"),
                _cacheService.RemoveAsync($"course_detail_{id}"),
                _cacheService.RemoveAsync($"course_{id}_enrollments"),
                _cacheService.RemoveAsync("courses_list_")
            );

            _logger.LogInformation("Invalidated course {CourseId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var result = await _decoratedService.DeleteCourseAsync(id);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"course_{id}"),
                    _cacheService.RemoveAsync($"course_detail_{id}"),
                    _cacheService.RemoveAsync($"course_{id}_enrollments"),
                    _cacheService.RemoveAsync("courses_list_")
                );

                _logger.LogInformation("Invalidated all course {CourseId} cache after deletion", id);
            }

            return result;
        }

        // Implement other ICourseService methods...
        public async Task<bool> AssignTeacherToCourseAsync(int courseId, int teacherId)
        {
            var result = await _decoratedService.AssignTeacherToCourseAsync(courseId, teacherId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"course_{courseId}"),
                    _cacheService.RemoveAsync($"course_detail_{courseId}"),
                    _cacheService.RemoveAsync("courses_list_")
                );
            }

            return result;
        }

        public async Task<bool> RemoveTeacherFromCourseAsync(int courseId)
        {
            var result = await _decoratedService.RemoveTeacherFromCourseAsync(courseId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"course_{courseId}"),
                    _cacheService.RemoveAsync($"course_detail_{courseId}"),
                    _cacheService.RemoveAsync("courses_list_")
                );
            }

            return result;
        }
    }
}
