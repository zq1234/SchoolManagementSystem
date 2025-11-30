using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> GetNotificationByIdAsync(int id);
        Task<APIResponseDto<NotificationDto>> GetNotificationsForUserAsync(string userId, string role, int page, int pageSize, string baseUrl);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(string userId, string role);
        Task<bool> DeleteNotificationAsync(int id);
        Task<NotificationStatsDto> GetNotificationStatsAsync(string userId, string role);
        Task<bool> SendBulkNotificationAsync(BulkNotificationDto bulkNotificationDto);

        Task<bool> SendAssignmentGradedEmailAsync(int submissionId);
        Task<bool> SendNewClassNotificationAsync(int classId);
    }
}
