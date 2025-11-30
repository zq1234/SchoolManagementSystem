using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        public NotificationService(
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
                throw new NotFoundException(nameof(Notification), id);

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<APIResponseDto<NotificationDto>> GetNotificationsForUserAsync(string userId, string role, int page, int pageSize, string baseUrl)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientRole == "All" ||
                           n.RecipientRole == role ||
                           (n.RecipientId != null && n.RecipientId.ToString() == userId))
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

            return new APIResponseDto<NotificationDto>(notificationDtos, page, pageSize, totalCount, baseUrl);
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            var notification = new Notification
            {
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                RecipientRole = createNotificationDto.RecipientRole,
                RecipientId = createNotificationDto.RecipientId,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification created with ID: {NotificationId}", notification.Id);

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                throw new NotFoundException(nameof(Notification), id);

            notification.IsRead = true;
            notification.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification marked as read: {NotificationId}", id);
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId, string role)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsRead &&
                           (n.RecipientRole == "All" ||
                            n.RecipientRole == role ||
                            (n.RecipientId != null && n.RecipientId.ToString() == userId)))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedDate = DateTime.UtcNow;
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", notifications.Count, userId);
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                throw new NotFoundException(nameof(Notification), id);

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Notification deleted with ID: {NotificationId}", id);
            return true;
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(string userId, string role)
        {
            var totalNotifications = await _context.Notifications
                .CountAsync(n => n.RecipientRole == "All" ||
                                n.RecipientRole == role ||
                                (n.RecipientId != null && n.RecipientId.ToString() == userId));

            var unreadNotifications = await _context.Notifications
                .CountAsync(n => !n.IsRead &&
                                (n.RecipientRole == "All" ||
                                 n.RecipientRole == role ||
                                 (n.RecipientId != null && n.RecipientId.ToString() == userId)));

            return new NotificationStatsDto
            {
                TotalNotifications = totalNotifications,
                UnreadNotifications = unreadNotifications,
                ReadNotifications = totalNotifications - unreadNotifications
            };
        }

        public async Task<bool> SendBulkNotificationAsync(BulkNotificationDto bulkNotificationDto)
        {
            var notifications = new List<Notification>();

            foreach (var recipientId in bulkNotificationDto.RecipientIds)
            {
                var notification = new Notification
                {
                    Title = bulkNotificationDto.Title,
                    Message = bulkNotificationDto.Message,
                    RecipientRole = bulkNotificationDto.RecipientRole,
                    RecipientId = recipientId,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                notifications.Add(notification);
            }

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Bulk notification sent to {Count} recipients", bulkNotificationDto.RecipientIds.Count);
            return true;
        }

        public async Task<bool> SendAssignmentGradedEmailAsync(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Include(s => s.GradedByTeacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException(nameof(Submission), submissionId);

            if (string.IsNullOrEmpty(submission.Grade))
                throw new BadRequestException("Submission has not been graded yet.");

            var studentEmail = submission.Student.User.Email;
            var studentName = $"{submission.Student.User.FirstName} {submission.Student.User.LastName}";
            var assignmentTitle = submission.Assignment.Title;
            var grade = submission.Grade;
            var teacherName = submission.GradedByTeacher != null ?
                $"{submission.GradedByTeacher.User.FirstName} {submission.GradedByTeacher.User.LastName}" : "Teacher";

            var subject = $"Your assignment '{assignmentTitle}' has been graded";
            var message = $"""
            Dear {studentName},

            Your submission for the assignment '{assignmentTitle}' has been graded.

            Grade: {grade}
            Graded by: {teacherName}
            Graded on: {DateTime.UtcNow:MMMM dd, yyyy}

            {(!string.IsNullOrEmpty(submission.Remarks) ? $"Remarks: {submission.Remarks}" : "")}

            You can view your grade and feedback in the student portal.

            Best regards,
            School Management System
            """;

            try
            {
                await _emailService.SendEmailAsync(studentEmail, subject, message);

                // Also create a notification in the system
                var notification = new Notification
                {
                    Title = "Assignment Graded",
                    Message = $"Your assignment '{assignmentTitle}' has been graded. Grade: {grade}",
                    RecipientRole = "Student",
                    RecipientId = submission.StudentId,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Assignment graded email sent to student {StudentId} for submission {SubmissionId}",
                    submission.StudentId, submissionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment graded email to student {StudentId}", submission.StudentId);
                return false;
            }
        }

        public async Task<bool> SendNewClassNotificationAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            // Get all students enrolled in this class
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            if (!enrollments.Any())
            {
                _logger.LogWarning("No students enrolled in class {ClassId} for notification", classId);
                return false;
            }

            var className = $"{classEntity.Name} - {classEntity.Section}";
            var courseName = classEntity.Course?.Name ?? "Unknown Course";
            var teacherName = classEntity.Teacher != null ?
                $"{classEntity.Teacher.User.FirstName} {classEntity.Teacher.User.LastName}" : "TBA";

            var successCount = 0;
            var failCount = 0;

            foreach (var enrollment in enrollments)
            {
                try
                {
                    var studentEmail = enrollment.Student.User.Email;
                    var studentName = $"{enrollment.Student.User.FirstName} {enrollment.Student.User.LastName}";

                    var subject = $"New Class: {className}";
                    var message = $"""
                    Dear {studentName},

                    You have been enrolled in a new class:

                    Class: {className}
                    Course: {courseName}
                    Teacher: {teacherName}
                    Semester: {classEntity.Semester}
                    Start Date: {classEntity.StartDate:MMMM dd, yyyy}
                    End Date: {classEntity.EndDate:MMMM dd, yyyy}

                    Please check your class schedule for more details.

                    Best regards,
                    School Management System
                    """;

                    await _emailService.SendEmailAsync(studentEmail, subject, message);

                    // Create system notification
                    var notification = new Notification
                    {
                        Title = "New Class Enrollment",
                        Message = $"You have been enrolled in {className} for {courseName}",
                        RecipientRole = "Student",
                        RecipientId = enrollment.StudentId,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false
                    };

                    await _unitOfWork.Notifications.AddAsync(notification);

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send new class notification to student {StudentId}", enrollment.StudentId);
                    failCount++;
                }
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("New class notifications sent for class {ClassId}: {SuccessCount} successful, {FailCount} failed",
                classId, successCount, failCount);

            return successCount > 0;
        }
    }
}
