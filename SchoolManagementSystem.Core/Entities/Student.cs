
using System.Diagnostics;

namespace SchoolManagementSystem.Core.Entities
{
    public class Student : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public string StudentId { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    }
}