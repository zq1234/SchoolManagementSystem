using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingEnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingEnrollmentService> _logger;

        private static readonly TimeSpan EnrollmentListCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan EnrollmentDetailCacheExpiration = TimeSpan.FromMinutes(15);

        public CachingEnrollmentService(
            IEnrollmentService decoratedService,
            ICacheService cacheService,
            ILogger<CachingEnrollmentService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<EnrollmentDto> GetEnrollmentByIdAsync(int id)
        {
            var cacheKey = $"enrollment_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetEnrollmentByIdAsync(id),
                EnrollmentDetailCacheExpiration);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetAllEnrollmentsAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"enrollments_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllEnrollmentsAsync(request, baseUrl),
                EnrollmentListCacheExpiration);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetEnrollmentsByStudentAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"course_{courseId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetEnrollmentsByCourseAsync(courseId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetEnrollmentsByClassAsync(classId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        // Write operations
        public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
        {
            var result = await _decoratedService.CreateEnrollmentAsync(createEnrollmentDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync("enrollments_list_"),
                _cacheService.RemoveAsync($"student_{createEnrollmentDto.StudentId}_enrollments"),
                _cacheService.RemoveAsync($"course_{createEnrollmentDto.CourseId}_enrollments"),
                _cacheService.RemoveAsync($"class_{createEnrollmentDto.ClassId}_enrollments")
            );

            _logger.LogInformation("Invalidated enrollment caches after creating new enrollment");
            return result;
        }

        public async Task<EnrollmentDto> UpdateEnrollmentAsync(int id, UpdateEnrollmentDto updateEnrollmentDto)
        {
            // Get existing enrollment to know what to invalidate
            var existingEnrollment = await _decoratedService.GetEnrollmentByIdAsync(id);

            var result = await _decoratedService.UpdateEnrollmentAsync(id, updateEnrollmentDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"enrollment_{id}"),
                _cacheService.RemoveAsync("enrollments_list_"),
                _cacheService.RemoveAsync($"student_{existingEnrollment.StudentId}_enrollments"),
                _cacheService.RemoveAsync($"course_{existingEnrollment.CourseId}_enrollments"),
                _cacheService.RemoveAsync($"class_{existingEnrollment.ClassId}_enrollments")
            );

            _logger.LogInformation("Invalidated enrollment {EnrollmentId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            // Get existing enrollment to know what to invalidate
            var existingEnrollment = await _decoratedService.GetEnrollmentByIdAsync(id);

            var result = await _decoratedService.DeleteEnrollmentAsync(id);

            if (result)
            {
                // Invalidate all enrollment-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"enrollment_{id}"),
                    _cacheService.RemoveAsync("enrollments_list_"),
                    _cacheService.RemoveAsync($"student_{existingEnrollment.StudentId}_enrollments"),
                    _cacheService.RemoveAsync($"course_{existingEnrollment.CourseId}_enrollments"),
                    _cacheService.RemoveAsync($"class_{existingEnrollment.ClassId}_enrollments")
                );

                _logger.LogInformation("Invalidated all enrollment {EnrollmentId} cache after deletion", id);
            }

            return result;
        }

        public async Task<bool> UpdateEnrollmentStatusAsync(int id, EnrollmentStatusDto statusDto)
        {
            // Get existing enrollment to know what to invalidate
            var existingEnrollment = await _decoratedService.GetEnrollmentByIdAsync(id);

            var result = await _decoratedService.UpdateEnrollmentStatusAsync(id, statusDto);

            if (result)
            {
                // Invalidate relevant caches
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"enrollment_{id}"),
                    _cacheService.RemoveAsync("enrollments_list_"),
                    _cacheService.RemoveAsync($"student_{existingEnrollment.StudentId}_enrollments"),
                    _cacheService.RemoveAsync($"course_{existingEnrollment.CourseId}_enrollments"),
                    _cacheService.RemoveAsync($"class_{existingEnrollment.ClassId}_enrollments")
                );

                _logger.LogInformation("Invalidated enrollment {EnrollmentId} cache after status update", id);
            }

            return result;
        }
    }
}