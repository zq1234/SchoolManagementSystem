using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingGradeService : IGradeService
    {
        private readonly IGradeService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingGradeService> _logger;

        private static readonly TimeSpan GradeListCacheExpiration = TimeSpan.FromMinutes(5); // Grades change frequently
        private static readonly TimeSpan GradeDetailCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan StudentCourseGradesCacheExpiration = TimeSpan.FromMinutes(5);

        public CachingGradeService(
            IGradeService decoratedService,
            ICacheService cacheService,
            ILogger<CachingGradeService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GradeDto> GetGradeByIdAsync(int id)
        {
            var cacheKey = $"grade_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetGradeByIdAsync(id),
                GradeDetailCacheExpiration);
        }

        //public async Task<IEnumerable<GradeDto>> GetAllGradesAsync()
        //{
        //    var cacheKey = "grades_all";
        //    return await _cacheService.GetOrCreateAsync(
        //        cacheKey,
        //        () => _decoratedService.GetAllGradesAsync(),
        //        GradeListCacheExpiration);
        //}

        public async Task<APIResponseDto<GradeDto>> GetAllGradesAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"grades_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllGradesAsync(request, baseUrl),
                GradeListCacheExpiration);
        }

        public async Task<APIResponseDto<GradeDto>> GetGradesByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"student_{studentId}_grades_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetGradesByStudentAsync(studentId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<APIResponseDto<GradeDto>> GetGradesByCourseAsync(int courseId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"course_{courseId}_grades_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetGradesByCourseAsync(courseId, request, baseUrl),
                TimeSpan.FromMinutes(5));
        }

        public async Task<StudentCourseGradeDto> GetStudentCourseGradesAsync(int studentId, int courseId)
        {
            var cacheKey = $"student_{studentId}_course_{courseId}_grades";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetStudentCourseGradesAsync(studentId, courseId),
                StudentCourseGradesCacheExpiration);
        }

        // Write operations
        public async Task<GradeDto> CreateGradeAsync(CreateGradeDto createGradeDto)
        {
            var result = await _decoratedService.CreateGradeAsync(createGradeDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync("grades_list_"),
                _cacheService.RemoveAsync("grades_all"),
                _cacheService.RemoveAsync($"student_{createGradeDto.StudentId}_grades"),
                _cacheService.RemoveAsync($"course_{createGradeDto.CourseId}_grades"),
                _cacheService.RemoveAsync($"student_{createGradeDto.StudentId}_course_{createGradeDto.CourseId}_grades")
            );

            _logger.LogInformation("Invalidated grade caches after creating new grade");
            return result;
        }

        public async Task<GradeDto> UpdateGradeAsync(int id, UpdateGradeDto updateGradeDto)
        {
            // Get existing grade to know what to invalidate
            var existingGrade = await _decoratedService.GetGradeByIdAsync(id);

            var result = await _decoratedService.UpdateGradeAsync(id, updateGradeDto);

            // Invalidate relevant caches
            await Task.WhenAll(
                _cacheService.RemoveAsync($"grade_{id}"),
                _cacheService.RemoveAsync("grades_list_"),
                _cacheService.RemoveAsync("grades_all"),
                _cacheService.RemoveAsync($"student_{existingGrade.StudentId}_grades"),
                _cacheService.RemoveAsync($"course_{existingGrade.CourseId}_grades"),
                _cacheService.RemoveAsync($"student_{existingGrade.StudentId}_course_{existingGrade.CourseId}_grades")
            );

            _logger.LogInformation("Invalidated grade {GradeId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteGradeAsync(int id)
        {
            // Get existing grade to know what to invalidate
            var existingGrade = await _decoratedService.GetGradeByIdAsync(id);

            var result = await _decoratedService.DeleteGradeAsync(id);

            if (result)
            {
                // Invalidate all grade-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"grade_{id}"),
                    _cacheService.RemoveAsync("grades_list_"),
                    _cacheService.RemoveAsync("grades_all"),
                    _cacheService.RemoveAsync($"student_{existingGrade.StudentId}_grades"),
                    _cacheService.RemoveAsync($"course_{existingGrade.CourseId}_grades"),
                    _cacheService.RemoveAsync($"student_{existingGrade.StudentId}_course_{existingGrade.CourseId}_grades")
                );

                _logger.LogInformation("Invalidated all grade {GradeId} cache after deletion", id);
            }

            return result;
        }

        public async Task<bool> BulkCreateGradesAsync(BulkCreateGradesDto bulkCreateGradesDto)
        {
            var result = await _decoratedService.BulkCreateGradesAsync(bulkCreateGradesDto);

            if (result)
            {
                // Invalidate course grades cache and student grades cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"course_{bulkCreateGradesDto.CourseId}_grades"),
                    _cacheService.RemoveAsync("grades_list_"),
                    _cacheService.RemoveAsync("grades_all")
                );

                // Also invalidate individual student course grades
                foreach (var gradeItem in bulkCreateGradesDto.Grades)
                {
                    await _cacheService.RemoveAsync($"student_{gradeItem.StudentId}_course_{bulkCreateGradesDto.CourseId}_grades");
                    await _cacheService.RemoveAsync($"student_{gradeItem.StudentId}_grades");
                }

                _logger.LogInformation("Invalidated grade caches after bulk grade creation for course {CourseId}", bulkCreateGradesDto.CourseId);
            }

            return result;
        }
    }
}