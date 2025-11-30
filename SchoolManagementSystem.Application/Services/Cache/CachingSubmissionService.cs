using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingSubmissionService : ISubmissionService
    {
        private readonly ISubmissionService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingSubmissionService> _logger;

        private static readonly TimeSpan SubmissionListCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan SubmissionDetailCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan SubmissionStatsCacheExpiration = TimeSpan.FromMinutes(5);

        public CachingSubmissionService(
            ISubmissionService decoratedService,
            ICacheService cacheService,
            ILogger<CachingSubmissionService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<SubmissionDto> GetSubmissionByIdAsync(int id)
        {
            var cacheKey = $"submission_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetSubmissionByIdAsync(id),
                SubmissionDetailCacheExpiration);
        }

        public async Task<APIResponseDto<SubmissionDto>> GetSubmissionsByAssignmentAsync(int assignmentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"assignment_{assignmentId}_submissions_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetSubmissionsByAssignmentAsync(assignmentId, request, baseUrl),
                SubmissionListCacheExpiration);
        }

        public async Task<APIResponseDto<SubmissionDto>> GetSubmissionsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_submissions_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetSubmissionsByStudentAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        public async Task<SubmissionStatsDto> GetSubmissionStatsAsync(int assignmentId)
        {
            var cacheKey = $"assignment_{assignmentId}_submission_stats";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetSubmissionStatsAsync(assignmentId),
                SubmissionStatsCacheExpiration);
        }

        // Write operations
        public async Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionDto createSubmissionDto, int studentId)
        {
            var result = await _decoratedService.SubmitAssignmentAsync(createSubmissionDto, studentId);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"assignment_{createSubmissionDto.AssignmentId}_submissions"),
                _cacheService.RemoveAsync($"student_{studentId}_submissions"),
                _cacheService.RemoveAsync($"assignment_{createSubmissionDto.AssignmentId}_submission_stats")
            );

            _logger.LogInformation("Invalidated submission caches after new submission for assignment {AssignmentId}", createSubmissionDto.AssignmentId);
            return result;
        }

        public async Task<SubmissionDto> UpdateSubmissionAsync(int id, UpdateSubmissionDto updateSubmissionDto)
        {
            // Get existing submission to know what to invalidate
            var existingSubmission = await _decoratedService.GetSubmissionByIdAsync(id);

            var result = await _decoratedService.UpdateSubmissionAsync(id, updateSubmissionDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"submission_{id}"),
                _cacheService.RemoveAsync($"assignment_{existingSubmission.AssignmentId}_submissions"),
                _cacheService.RemoveAsync($"student_{existingSubmission.StudentId}_submissions")
            );

            _logger.LogInformation("Invalidated submission {SubmissionId} cache after update", id);
            return result;
        }

        public async Task<SubmissionDto> GradeSubmissionAsync(int id, GradeSubmissionDto gradeSubmissionDto, int teacherId)
        {
            // Get existing submission to know what to invalidate
            var existingSubmission = await _decoratedService.GetSubmissionByIdAsync(id);

            var result = await _decoratedService.GradeSubmissionAsync(id, gradeSubmissionDto, teacherId);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"submission_{id}"),
                _cacheService.RemoveAsync($"assignment_{existingSubmission.AssignmentId}_submissions"),
                _cacheService.RemoveAsync($"student_{existingSubmission.StudentId}_submissions"),
                _cacheService.RemoveAsync($"assignment_{existingSubmission.AssignmentId}_submission_stats")
            );

            _logger.LogInformation("Invalidated submission {SubmissionId} cache after grading", id);
            return result;
        }

        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            // Get existing submission to know what to invalidate
            var existingSubmission = await _decoratedService.GetSubmissionByIdAsync(id);

            var result = await _decoratedService.DeleteSubmissionAsync(id);

            if (result)
            {
                // Invalidate all submission-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"submission_{id}"),
                    _cacheService.RemoveAsync($"assignment_{existingSubmission.AssignmentId}_submissions"),
                    _cacheService.RemoveAsync($"student_{existingSubmission.StudentId}_submissions"),
                    _cacheService.RemoveAsync($"assignment_{existingSubmission.AssignmentId}_submission_stats")
                );

                _logger.LogInformation("Invalidated all submission {SubmissionId} cache after deletion", id);
            }

            return result;
        }
    }
}