using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Get User

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;

            return userDto;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            var roles = await _userManager.GetRolesAsync(user);
            var profileDto = _mapper.Map<UserProfileDto>(user);
            profileDto.Roles = roles;

            if (roles.Contains("Student"))
            {
                var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == id);
                profileDto.StudentId = student?.StudentId;
                profileDto.EnrollmentDate = student?.EnrollmentDate;
            }
            else if (roles.Contains("Teacher"))
            {
                var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == id);
                profileDto.EmployeeId = teacher?.EmployeeId;
                profileDto.Department = teacher?.Department;
                profileDto.Qualification = teacher?.Qualification;
                profileDto.HireDate = teacher?.HireDate;
            }

            return profileDto;
        }

        #endregion

        #region Pagination

        public async Task<APIResponseDto<UserDto>> GetAllUsersAsync(SearchRequestDto dto, string baseUrl)
        {
            var query = _userManager.Users.Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(dto.Search) ||
                    u.LastName.Contains(dto.Search) ||
                    u.Email.Contains(dto.Search) ||
                    u.PhoneNumber.Contains(dto.Search));
            }

            // Apply sorting
            query = dto.SortBy?.ToLower() switch
            {
                "name" => dto.SortDescending ?
                    query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName) :
                    query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "email" => dto.SortDescending ?
                    query.OrderByDescending(u => u.Email) :
                    query.OrderBy(u => u.Email),
                "createddate" => dto.SortDescending ?
                    query.OrderByDescending(u => u.CreatedDate) :
                    query.OrderBy(u => u.CreatedDate),
                _ => query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            };

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles;
                userDtos.Add(userDto);
            }

            return new APIResponseDto<UserDto>(
                message: "Users retrieved successfully",
                data: userDtos,
                page: dto.Page,
                pageSize: dto.PageSize,
                totalCount: totalCount,
                baseUrl: baseUrl
            );
        }

        public async Task<APIResponseDto<UserDto>> GetUsersByRoleAsync(string role, SearchRequestDto dto, string baseUrl)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists) throw new BadRequestException($"Role '{role}' does not exist");

            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var activeUsers = usersInRole.Where(u => u.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                activeUsers = activeUsers.Where(u =>
                    u.FirstName.Contains(dto.Search) ||
                    u.LastName.Contains(dto.Search) ||
                    u.Email.Contains(dto.Search));
            }

            var totalCount = activeUsers.Count();

            var pagedUsers = activeUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles;
                userDtos.Add(userDto);
            }

            return new APIResponseDto<UserDto>(
                userDtos,
                dto.Page,
                dto.PageSize,
                totalCount,
                baseUrl,
                $"Users with role '{role}' retrieved successfully"
            );
        }

        public async Task<APIResponseDto<UserDto>> SearchUsersAsync(string query, SearchRequestDto dto, string baseUrl)
        {
            var usersQuery = _userManager.Users.Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(query) ||
                    u.LastName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.PhoneNumber.Contains(query) ||
                    (u.FirstName + " " + u.LastName).Contains(query));
            }

            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles;
                userDtos.Add(userDto);
            }

            return new APIResponseDto<UserDto>(
                userDtos,
                dto.Page,
                dto.PageSize,
                totalCount,
                baseUrl,
                "Users search completed successfully"
            );
        }

        #endregion

        #region Update/Delete User

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;

            _logger.LogInformation("User updated: {UserId}", id);
            return userDto;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("User deleted: {UserId}", id);
            return true;
        }

        public async Task<bool> ActivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.IsActive = true;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("User activated: {UserId}", id);
            return true;
        }

        public async Task<bool> DeactivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User", id);

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("User deactivated: {UserId}", id);
            return true;
        }

        public async Task<int> BulkDeleteUsersAsync(List<string> userIds)
        {
            var successfulDeletes = 0;

            foreach (var userId in userIds)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.IsActive = false;
                        user.UpdatedDate = DateTime.UtcNow;
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            successfulDeletes++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete user {UserId}", userId);
                }
            }

            _logger.LogInformation("Bulk delete completed: {Successful}/{Total}", successfulDeletes, userIds.Count);
            return successfulDeletes;
        }

        #endregion

        #region Roles Management

        public async Task<bool> UpdateUserRolesAsync(string userId, UpdateUserRolesDto updateUserRolesDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            // Validate all roles exist
            foreach (var role in updateUserRolesDto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new BadRequestException($"Role '{role}' does not exist");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) throw new BadRequestException(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

            var addResult = await _userManager.AddToRolesAsync(user, updateUserRolesDto.Roles);
            if (!addResult.Succeeded) throw new BadRequestException(string.Join(", ", addResult.Errors.Select(e => e.Description)));

            _logger.LogInformation("User roles updated for {UserId}: {Roles}", userId, string.Join(", ", updateUserRolesDto.Roles));
            return true;
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) throw new BadRequestException($"Role '{roleName}' does not exist");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);
            return true;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return _mapper.Map<List<RoleDto>>(roles);
        }

        public async Task<RoleDto> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new BadRequestException("Role name cannot be empty");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists) throw new BadRequestException($"Role '{roleName}' already exists");

            var role = new IdentityRole(roleName.Trim());
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Role created: {RoleName}", roleName);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<bool> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) throw new NotFoundException("Role", roleName);

            // Check if role has users
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any()) throw new BadRequestException($"Cannot delete role '{roleName}' because it has assigned users");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Role deleted: {RoleName}", roleName);
            return true;
        }

        public async Task<RoleDto> UpdateRoleAsync(string oldRoleName, string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(newRoleName))
                throw new BadRequestException("New role name cannot be empty");

            var role = await _roleManager.FindByNameAsync(oldRoleName);
            if (role == null) throw new NotFoundException("Role", oldRoleName);

            var newRoleExists = await _roleManager.RoleExistsAsync(newRoleName);
            if (newRoleExists) throw new BadRequestException($"Role '{newRoleName}' already exists");

            role.Name = newRoleName.Trim();
            role.NormalizedName = newRoleName.Trim().ToUpper();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded) throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation("Role updated: {OldRoleName} to {NewRoleName}", oldRoleName, newRoleName);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<List<UserDto>> GetUsersInRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) throw new BadRequestException($"Role '{roleName}' does not exist");

            var users = await _userManager.GetUsersInRoleAsync(roleName);
            var activeUsers = users.Where(u => u.IsActive).ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in activeUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles;
                userDtos.Add(userDto);
            }

            return userDtos;
        }

        #endregion

        #region User Statistics

        public async Task<UserStatsDto> GetUserStatisticsAsync()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;

            var roleStats = new Dictionary<string, int>();
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleStats[role.Name] = usersInRole.Count(u => u.IsActive);
            }

            // Get recent registrations (last 30 days)
            var recentRegistrations = await _userManager.Users
                .Where(u => u.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                .CountAsync();

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                UsersByRole = roleStats,
                RecentRegistrations = recentRegistrations,
                LastUpdated = DateTime.UtcNow
            };
        }

        #endregion
    }
}