using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.API.Extensions;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region User Management

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(new ApiResponse<UserDto>(user, "User retrieved successfully"));
        }

        [HttpGet]
        [ProducesResponseType(typeof(APIResponseDto<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] SearchRequestDto dto)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var users = await _userService.GetAllUsersAsync(dto, baseUrl);
            return Ok(users);
        }

        [HttpGet("profile/{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            var userProfile = await _userService.GetUserProfileAsync(id);
            return Ok(new ApiResponse<UserProfileDto>(userProfile, "User profile retrieved successfully"));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(new ApiResponse<UserDto>(user, "User updated successfully"));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Ok(new ApiResponse<bool>(result, "User deleted successfully"));
        }

        [HttpPost("{id}/activate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var result = await _userService.ActivateUserAsync(id);
            return Ok(new ApiResponse<bool>(result, "User activated successfully"));
        }

        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            return Ok(new ApiResponse<bool>(result, "User deactivated successfully"));
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(APIResponseDto<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] SearchRequestDto dto)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var users = await _userService.SearchUsersAsync(query, dto, baseUrl);
            return Ok(users);
        }

        [HttpPost("bulk-delete")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkDeleteUsers([FromBody] List<string> userIds)
        {
            var count = await _userService.BulkDeleteUsersAsync(userIds);
            return Ok(new ApiResponse<int>(count, $"{count} users deleted successfully"));
        }

        #endregion

        #region Role Management

        [HttpPut("{userId}/roles")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateUserRolesDto updateUserRolesDto)
        {
            var result = await _userService.UpdateUserRolesAsync(userId, updateUserRolesDto);
            return Ok(new ApiResponse<bool>(result, "User roles updated successfully"));
        }

        [HttpPost("{userId}/roles/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var result = await _userService.AssignRoleToUserAsync(userId, roleName);
            return Ok(new ApiResponse<bool>(result, "Role assigned successfully"));
        }

        [HttpDelete("{userId}/roles/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var result = await _userService.RemoveRoleFromUserAsync(userId, roleName);
            return Ok(new ApiResponse<bool>(result, "Role removed successfully"));
        }

        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(APIResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsersByRole(string role, [FromQuery] SearchRequestDto dto)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var users = await _userService.GetUsersByRoleAsync(role, dto, baseUrl);
            return Ok(users);
        }

        #endregion

        #region Role CRUD Operations

        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userService.GetAllRolesAsync();
            return Ok(new ApiResponse<List<RoleDto>>(roles, "Roles retrieved successfully"));
        }

        [HttpPost("roles")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            var role = await _userService.CreateRoleAsync(createRoleDto.Name);
            return Ok(new ApiResponse<RoleDto>(role, "Role created successfully"));
        }

        [HttpDelete("roles/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _userService.DeleteRoleAsync(roleName);
            return Ok(new ApiResponse<bool>(result, "Role deleted successfully"));
        }

        [HttpPut("roles/{oldRoleName}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRole(string oldRoleName, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var role = await _userService.UpdateRoleAsync(oldRoleName, updateRoleDto.NewName);
            return Ok(new ApiResponse<RoleDto>(role, "Role updated successfully"));
        }

        [HttpGet("roles/{roleName}/users")]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsersInRole(string roleName)
        {
            var users = await _userService.GetUsersInRoleAsync(roleName);
            return Ok(new ApiResponse<List<UserDto>>(users, $"Users in role '{roleName}' retrieved successfully"));
        }

        #endregion

        #region Statistics

        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<UserStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserStatistics()
        {
            var stats = await _userService.GetUserStatisticsAsync();
            return Ok(new ApiResponse<UserStatsDto>(stats, "User statistics retrieved successfully"));
        }

        #endregion

        [HttpGet("my-profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.GetUserId();
            var profile = await _userService.GetUserProfileAsync(userId);
            return Ok(new ApiResponse<UserProfileDto>(profile, "Your profile retrieved successfully"));
        }
    }
}