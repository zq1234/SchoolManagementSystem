using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IUserService
    {
        // User Management
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserProfileDto> GetUserProfileAsync(string id);
        Task<APIResponseDto<UserDto>> GetAllUsersAsync(SearchRequestDto dto, string baseUrl);
        Task<APIResponseDto<UserDto>> GetUsersByRoleAsync(string role, SearchRequestDto dto, string baseUrl);
        Task<APIResponseDto<UserDto>> SearchUsersAsync(string query, SearchRequestDto dto, string baseUrl);

        // User CRUD Operations
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ActivateUserAsync(string id);
        Task<bool> DeactivateUserAsync(string id);
        Task<int> BulkDeleteUsersAsync(List<string> userIds);

        // Role Management
        Task<bool> UpdateUserRolesAsync(string userId, UpdateUserRolesDto updateUserRolesDto);
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);

        // Role CRUD Operations
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleName);
        Task<RoleDto> UpdateRoleAsync(string oldRoleName, string newRoleName);
        Task<List<UserDto>> GetUsersInRoleAsync(string roleName);

        // Statistics
        Task<UserStatsDto> GetUserStatisticsAsync();
    }
}