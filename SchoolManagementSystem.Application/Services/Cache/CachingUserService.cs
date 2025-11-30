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
    public class CachingUserService : IUserService
    {
        private readonly IUserService _decoratedService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachingUserService> _logger;

        // Cache expiration times
        private static readonly TimeSpan UserListCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan UserDetailCacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan UserProfileCacheExpiration = TimeSpan.FromMinutes(20);
        private static readonly TimeSpan RolesCacheExpiration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan StatisticsCacheExpiration = TimeSpan.FromMinutes(5);

        public CachingUserService(
            IUserService decoratedService,
            ICacheService cacheService,
            ILogger<CachingUserService> logger)
        {
            _decoratedService = decoratedService;
            _cacheService = cacheService;
            _logger = logger;
        }

        #region Get User Methods

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var cacheKey = $"user_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetUserByIdAsync(id),
                UserDetailCacheExpiration);
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string id)
        {
            var cacheKey = $"user_profile_{id}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetUserProfileAsync(id),
                UserProfileCacheExpiration);
        }

        public async Task<APIResponseDto<UserDto>> GetAllUsersAsync(SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"users_list_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllUsersAsync(request, baseUrl),
                UserListCacheExpiration);
        }

        public async Task<APIResponseDto<UserDto>> GetUsersByRoleAsync(string role, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"users_by_role_{role}_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetUsersByRoleAsync(role, request, baseUrl),
                UserListCacheExpiration);
        }

        public async Task<APIResponseDto<UserDto>> SearchUsersAsync(string query, SearchRequestDto request, string baseUrl)
        {
            var cacheKey = $"users_search_{query}_{request.Search}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.SortDescending}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.SearchUsersAsync(query, request, baseUrl),
                UserListCacheExpiration);
        }

        #endregion

        #region Update/Delete User

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var result = await _decoratedService.UpdateUserAsync(id, updateUserDto);

            await InvalidateUserCache(id);
            _logger.LogInformation("Invalidated user {UserId} cache after update", id);

            return result;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _decoratedService.DeleteUserAsync(id);

            if (result)
            {
                await InvalidateUserCache(id);
                _logger.LogInformation("Invalidated all user {UserId} cache after deletion", id);
            }

            return result;
        }

        public async Task<bool> ActivateUserAsync(string id)
        {
            var result = await _decoratedService.ActivateUserAsync(id);

            if (result)
            {
                await InvalidateUserCache(id);
                _logger.LogInformation("Invalidated user {UserId} cache after activation", id);
            }

            return result;
        }

        public async Task<bool> DeactivateUserAsync(string id)
        {
            var result = await _decoratedService.DeactivateUserAsync(id);

            if (result)
            {
                await InvalidateUserCache(id);
                _logger.LogInformation("Invalidated user {UserId} cache after deactivation", id);
            }

            return result;
        }

        public async Task<int> BulkDeleteUsersAsync(List<string> userIds)
        {
            var result = await _decoratedService.BulkDeleteUsersAsync(userIds);

            if (result > 0)
            {
                await InvalidateAllUserLists();
                _logger.LogInformation("Invalidated all user lists cache after bulk deletion of {Count} users", result);
            }

            return result;
        }

        #endregion

        #region Roles Management

        public async Task<bool> UpdateUserRolesAsync(string userId, UpdateUserRolesDto updateUserRolesDto)
        {
            var result = await _decoratedService.UpdateUserRolesAsync(userId, updateUserRolesDto);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync($"user_{userId}"),
                    _cacheService.RemoveAsync($"user_profile_{userId}"),
                    _cacheService.RemoveByPatternAsync("users_list_*"),
                    _cacheService.RemoveByPatternAsync("users_by_role_*"),
                    _cacheService.RemoveByPatternAsync("users_search_*")
                );

                _logger.LogInformation("Invalidated user {UserId} cache after role update", userId);
            }

            return result;
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var result = await _decoratedService.AssignRoleToUserAsync(userId, roleName);

            if (result)
            {
                await InvalidateUserCache(userId);
                _logger.LogInformation("Invalidated user {UserId} cache after role assignment", userId);
            }

            return result;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var result = await _decoratedService.RemoveRoleFromUserAsync(userId, roleName);

            if (result)
            {
                await InvalidateUserCache(userId);
                _logger.LogInformation("Invalidated user {UserId} cache after role removal", userId);
            }

            return result;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var cacheKey = "all_roles";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetAllRolesAsync(),
                RolesCacheExpiration);
        }

        public async Task<RoleDto> CreateRoleAsync(string roleName)
        {
            var result = await _decoratedService.CreateRoleAsync(roleName);

            // Invalidate roles cache
            await _cacheService.RemoveAsync("all_roles");
            _logger.LogInformation("Invalidated roles cache after creating new role: {RoleName}", roleName);

            return result;
        }

        public async Task<bool> DeleteRoleAsync(string roleName)
        {
            var result = await _decoratedService.DeleteRoleAsync(roleName);

            if (result)
            {
                await Task.WhenAll(
                    _cacheService.RemoveAsync("all_roles"),
                    _cacheService.RemoveByPatternAsync("users_by_role_*"),
                    _cacheService.RemoveByPatternAsync("users_list_*")
                );

                _logger.LogInformation("Invalidated roles and user lists cache after deleting role: {RoleName}", roleName);
            }

            return result;
        }

        public async Task<RoleDto> UpdateRoleAsync(string oldRoleName, string newRoleName)
        {
            var result = await _decoratedService.UpdateRoleAsync(oldRoleName, newRoleName);

            // Invalidate roles cache and role-based user lists
            await Task.WhenAll(
                _cacheService.RemoveAsync("all_roles"),
                _cacheService.RemoveByPatternAsync($"users_by_role_{oldRoleName}_*"),
                _cacheService.RemoveByPatternAsync("users_list_*")
            );

            _logger.LogInformation("Invalidated roles cache after updating role: {OldRoleName} to {NewRoleName}", oldRoleName, newRoleName);

            return result;
        }

        public async Task<List<UserDto>> GetUsersInRoleAsync(string roleName)
        {
            var cacheKey = $"users_in_role_{roleName}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetUsersInRoleAsync(roleName),
                UserListCacheExpiration);
        }

        #endregion

        #region Statistics

        public async Task<UserStatsDto> GetUserStatisticsAsync()
        {
            var cacheKey = "user_statistics";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                () => _decoratedService.GetUserStatisticsAsync(),
                StatisticsCacheExpiration);
        }

        #endregion

        #region Helper Methods for Cache Invalidation

        private async Task InvalidateUserCache(string userId)
        {
            await Task.WhenAll(
                _cacheService.RemoveAsync($"user_{userId}"),
                _cacheService.RemoveAsync($"user_profile_{userId}"),
                _cacheService.RemoveByPatternAsync("users_list_*"),
                _cacheService.RemoveByPatternAsync("users_by_role_*"),
                _cacheService.RemoveByPatternAsync("users_search_*"),
                _cacheService.RemoveAsync("user_statistics")
            );
        }

        private async Task InvalidateAllUserLists()
        {
            await Task.WhenAll(
                _cacheService.RemoveByPatternAsync("users_list_*"),
                _cacheService.RemoveByPatternAsync("users_by_role_*"),
                _cacheService.RemoveByPatternAsync("users_search_*"),
                _cacheService.RemoveAsync("user_statistics")
            );
        }

        #endregion
    }
}