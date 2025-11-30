using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Infrastructure.Services;
using System.Security.Claims;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            return Ok(new ApiResponse<NotificationDto>(notification, "Notification retrieved successfully"));
        }

        [HttpGet("user/{userId}/{role}")]
        public async Task<IActionResult> GetNotificationsForUser(string userId, string role, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var notifications = await _notificationService.GetNotificationsForUserAsync(userId, role, page, pageSize, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<NotificationDto>>(notifications, "User notifications retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
        {
            var notification = await _notificationService.CreateNotificationAsync(createNotificationDto);
            return CreatedAtAction(
                nameof(GetNotificationById),
                new { id = notification.Id },
                new ApiResponse<NotificationDto>(notification, "Notification created successfully")
            );
        }

        [HttpPatch("{id}/mark-read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(new ApiResponse<bool>(result, "Notification marked as read successfully"));
        }

        [HttpPost("mark-all-read/{userId}/{role}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead(string userId, string role)
        {
            var result = await _notificationService.MarkAllAsReadAsync(userId, role);
            return Ok(new ApiResponse<bool>(result, "All notifications marked as read successfully"));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Ok(new ApiResponse<bool>(result, "Notification deleted successfully"));
        }

        [HttpGet("stats/{userId}/{role}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificationStats(string userId, string role)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var role = User.FindFirst(ClaimTypes.Role)?.Value;

            //if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            //    throw new UnauthorizedException("User information not found in token");

            var stats = await _notificationService.GetNotificationStatsAsync(userId, role);
            return Ok(new ApiResponse<NotificationStatsDto>(stats, "Notification stats retrieved successfully"));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationDto bulkNotificationDto)
        {
            var result = await _notificationService.SendBulkNotificationAsync(bulkNotificationDto);
            return Ok(new ApiResponse<bool>(result, "Bulk notification sent successfully"));
        }




        [HttpPost("assignment-graded/{submissionId}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendAssignmentGradedEmail(int submissionId)
        {
            var result = await _notificationService.SendAssignmentGradedEmailAsync(submissionId);
            return Ok(new ApiResponse<bool>(result, "Assignment graded email sent successfully"));
        }

        [HttpPost("new-class/{classId}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendNewClassNotification(int classId)
        {
            var result = await _notificationService.SendNewClassNotificationAsync(classId);
            return Ok(new ApiResponse<bool>(result, "New class notifications sent successfully"));
        }

        [HttpGet("my-notifications")]
        [Authorize(Roles = "Student,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                throw new UnauthorizedException("User information not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var notifications = await _notificationService.GetNotificationsForUserAsync(userId, role, page, pageSize, baseUrl);
            return Ok(notifications);
            //return Ok(new ApiResponse<APIResponseDto<NotificationDto>>(notifications, "Your notifications retrieved successfully"));
        }

        [HttpPost("my-notifications/mark-all-read")]
        [Authorize(Roles = "Student,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllMyNotificationsAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                throw new UnauthorizedException("User information not found in token");

            var result = await _notificationService.MarkAllAsReadAsync(userId, role);
            return Ok(new ApiResponse<bool>(result, "All your notifications marked as read successfully"));
        }

        [HttpGet("my-stats")]
        [Authorize(Roles = "Student,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<NotificationStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotificationStats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                throw new UnauthorizedException("User information not found in token");

            var stats = await _notificationService.GetNotificationStatsAsync(userId, role);
            return Ok(new ApiResponse<NotificationStatsDto>(stats, "Your notification stats retrieved successfully"));
        }
    }
}