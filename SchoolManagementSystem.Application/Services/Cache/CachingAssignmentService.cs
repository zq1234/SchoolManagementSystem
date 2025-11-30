using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingAssignmentService : IAssignmentService
    {
        private readonly IAssignmentService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingAssignmentService> _logger;

        private static readonly TimeSpan AssignmentListCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AssignmentDetailCacheExpiration = TimeSpan.FromMinutes(20);

        public CachingAssignmentService(
            IAssignmentService decoratedService,
            ICacheService cacheService,
            ILogger<CachingAssignmentService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AssignmentDto> GetAssignmentByIdAsync(int id)
        {
            var cacheKey = $"assignment_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAssignmentByIdAsync(id),
                AssignmentDetailCacheExpiration);
        }

        public async Task<AssignmentDetailDto> GetAssignmentDetailAsync(int id)
        {
            var cacheKey = $"assignment_detail_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAssignmentDetailAsync(id),
                AssignmentDetailCacheExpiration);
        }

        public async Task<APIResponseDto<AssignmentDto>> GetAllAssignmentsAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"assignments_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllAssignmentsAsync(request, baseUrl),
                AssignmentListCacheExpiration);
        }

        public async Task<APIResponseDto<AssignmentDto>> GetAssignmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_assignments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAssignmentsByClassAsync(classId, request, baseUrl),
                TimeSpan.FromMinutes(15));
        }

        public async Task<APIResponseDto<AssignmentDto>> GetAssignmentsByTeacherAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"teacher_{teacherId}_assignments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAssignmentsByTeacherAsync(teacherId, request, baseUrl),
                TimeSpan.FromMinutes(15));
        }

        // Write operations
        public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentDto createAssignmentDto, int teacherId)
        {
            var result = await _decoratedService.CreateAssignmentAsync(createAssignmentDto, teacherId);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync("assignments_list_"),
                _cacheService.RemoveAsync($"class_{createAssignmentDto.ClassId}_assignments"),
                _cacheService.RemoveAsync($"teacher_{teacherId}_assignments")
            );

            _logger.LogInformation("Invalidated assignment caches after creating new assignment");
            return result;
        }

        public async Task<AssignmentDto> UpdateAssignmentAsync(int id, UpdateAssignmentDto updateAssignmentDto)
        {
            // Get existing assignment to know what to invalidate
            var existingAssignment = await _decoratedService.GetAssignmentByIdAsync(id);

            var result = await _decoratedService.UpdateAssignmentAsync(id, updateAssignmentDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"assignment_{id}"),
                _cacheService.RemoveAsync($"assignment_detail_{id}"),
                _cacheService.RemoveAsync("assignments_list_"),
                _cacheService.RemoveAsync($"class_{existingAssignment.ClassId}_assignments"),
                _cacheService.RemoveAsync($"teacher_{existingAssignment.CreatedByTeacherId}_assignments")
            );

            _logger.LogInformation("Invalidated assignment {AssignmentId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            // Get existing assignment to know what to invalidate
            var existingAssignment = await _decoratedService.GetAssignmentByIdAsync(id);

            var result = await _decoratedService.DeleteAssignmentAsync(id);

            if (result)
            {
                // Invalidate all assignment-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"assignment_{id}"),
                    _cacheService.RemoveAsync($"assignment_detail_{id}"),
                    _cacheService.RemoveAsync("assignments_list_"),
                    _cacheService.RemoveAsync($"class_{existingAssignment.ClassId}_assignments"),
                    _cacheService.RemoveAsync($"teacher_{existingAssignment.CreatedByTeacherId}_assignments")
                );

                _logger.LogInformation("Invalidated all assignment {AssignmentId} cache after deletion", id);
            }

            return result;
        }
    }
}