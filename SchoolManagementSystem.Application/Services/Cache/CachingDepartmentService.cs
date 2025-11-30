using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class CachingDepartmentService : IDepartmentService
    {
        private readonly IDepartmentService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingDepartmentService> _logger;

        // Cache expiration times
        private static readonly TimeSpan DepartmentListCacheExpiration = TimeSpan.FromMinutes(20); // Departments change rarely
        private static readonly TimeSpan DepartmentDetailCacheExpiration = TimeSpan.FromMinutes(30);

        public CachingDepartmentService(
            IDepartmentService decoratedService,
            ICacheService cacheService,
            ILogger<CachingDepartmentService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<DepartmentDto> GetByIdAsync(int id)
        {
            var cacheKey = $"department_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetByIdAsync(id),
                DepartmentDetailCacheExpiration);
        }

        public async Task<APIResponseDto<DepartmentDto>> GetAllAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"departments_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllAsync(request, baseUrl),
                DepartmentListCacheExpiration);
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto request)
        {
            var result = await _decoratedService.CreateAsync(request);

            // Invalidate departments list cache
            await _cacheService.RemoveAsync("departments_list_");
            _logger.LogInformation("Invalidated departments list cache after creating new department");

            return result;
        }

        public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto request)
        {
            var result = await _decoratedService.UpdateAsync(id, request);

            // Invalidate specific department cache and lists
            await Task.WhenAll(
                _cacheService.RemoveAsync($"department_{id}"),
                _cacheService.RemoveAsync("departments_list_")
            );

            _logger.LogInformation("Invalidated department {DepartmentId} cache after update", id);
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _decoratedService.DeleteAsync(id);

            if (result)
            {
                // Invalidate all department-related cache
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"department_{id}"),
                    _cacheService.RemoveAsync("departments_list_")
                );

                _logger.LogInformation("Invalidated all department {DepartmentId} cache after deletion", id);
            }

            return result;
        }

        public async Task<DepartmentDto> AssignHeadOfDepartmentAsync(int departmentId, int teacherId)
        {
            var result = await _decoratedService.AssignHeadOfDepartmentAsync(departmentId, teacherId);

            if (result != null)
            {
                // Invalidate department cache when HOD changes
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"department_{departmentId}"),
                    _cacheService.RemoveAsync("departments_list_")
                );

                _logger.LogInformation("Invalidated department {DepartmentId} cache after HOD assignment", departmentId);
            }

            return result;
        }
    }
}