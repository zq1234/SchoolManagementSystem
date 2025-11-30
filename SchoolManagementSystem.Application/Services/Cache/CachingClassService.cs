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
    public class CachingClassService : IClassService
    {
        private readonly IClassService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingClassService> _logger;

        private static readonly TimeSpan ClassListCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ClassDetailCacheExpiration = TimeSpan.FromMinutes(20);

        public CachingClassService(
            IClassService decoratedService,
            ICacheService cacheService,
            ILogger<CachingClassService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ClassDto> GetClassByIdAsync(int id)
        {
            var cacheKey = $"class_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassByIdAsync(id),
                ClassDetailCacheExpiration);
        }

        public async Task<ClassDetailDto> GetClassDetailAsync(int id)
        {
            var cacheKey = $"class_detail_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassDetailAsync(id),
                ClassDetailCacheExpiration);
        }

        public async Task<APIResponseDto<ClassDto>> GetAllClassesAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"classes_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllClassesAsync(request, baseUrl),
                ClassListCacheExpiration);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetClassEnrollmentsAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"class_{classId}_enrollments_{request.Search}_{request.Page}_{request.PageSize}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetClassEnrollmentsAsync(classId, request, baseUrl),
                TimeSpan.FromMinutes(10));
        }

        // Write operations
        public async Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            var result = await _decoratedService.CreateClassAsync(createClassDto);

            await _cacheService.RemoveAsync("classes_list_");
            _logger.LogInformation("Invalidated classes list cache after creating new class");

            return result;
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto)
        {
            var result = await _decoratedService.UpdateClassAsync(id, updateClassDto);

            await Task.WhenAll(
                _cacheService.RemoveAsync($"class_{id}"),
                _cacheService.RemoveAsync($"class_detail_{id}"),
                _cacheService.RemoveAsync($"class_{id}_enrollments"),
                _cacheService.RemoveAsync("classes_list_")
            );

            _logger.LogInformation("Invalidated class {ClassId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            var result = await _decoratedService.DeleteClassAsync(id);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"class_{id}"),
                    _cacheService.RemoveAsync($"class_detail_{id}"),
                    _cacheService.RemoveAsync($"class_{id}_enrollments"),
                    _cacheService.RemoveAsync("classes_list_")
                );

                _logger.LogInformation("Invalidated all class {ClassId} cache after deletion", id);
            }

            return result;
        }

        // Implement other IClassService methods...
        public async Task<bool> AssignTeacherToClassAsync(int classId, int teacherId)
        {
            var result = await _decoratedService.AssignTeacherToClassAsync(classId, teacherId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"class_{classId}"),
                    _cacheService.RemoveAsync($"class_detail_{classId}"),
                    _cacheService.RemoveAsync("classes_list_")
                );
            }

            return result;
        }

        public async Task<bool> RemoveTeacherFromClassAsync(int classId)
        {
            var result = await _decoratedService.RemoveTeacherFromClassAsync(classId);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"class_{classId}"),
                    _cacheService.RemoveAsync($"class_detail_{classId}"),
                    _cacheService.RemoveAsync("classes_list_")
                );
            }

            return result;
        }
    }
}
