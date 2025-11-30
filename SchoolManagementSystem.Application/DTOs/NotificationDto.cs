using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string RecipientRole { get; set; } = string.Empty;
        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TimeAgo => GetTimeAgo(CreatedAt);

        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";

            return "Just now";
        }
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string RecipientRole { get; set; } = string.Empty; // All, Student, Teacher, Admin
        public int? RecipientId { get; set; }
    }

    public class BulkNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string RecipientRole { get; set; } = string.Empty;
        public List<int> RecipientIds { get; set; } = new();
    }

    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ReadNotifications { get; set; }
        public decimal ReadPercentage => TotalNotifications > 0 ? (decimal)ReadNotifications / TotalNotifications * 100 : 0;
    }

    public class MarkNotificationsReadDto
    {
        public List<int> NotificationIds { get; set; } = new();
    }
}
