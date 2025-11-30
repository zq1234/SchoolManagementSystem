using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
     

    public class StudentDashboardDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime GeneratedAt { get; set; }

        // Statistics
        public int CurrentCourses { get; set; }
        public int CompletedCourses { get; set; }
        public decimal OverallGPA { get; set; }

        // Recent Activity
        public List<GradeDto> RecentGrades { get; set; } = new();
        public List<AssignmentDto> UpcomingAssignments { get; set; } = new();
        public List<AssignmentDto> PendingSubmissions { get; set; } = new();
        public List<NotificationDto> RecentNotifications { get; set; } = new();

        // Performance
        public List<CoursePerformanceDto> CoursePerformance { get; set; } = new();
        public AttendanceSummaryDto RecentAttendance { get; set; } = new();
    }

    public class CoursePerformanceDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public decimal AverageGrade { get; set; }
        public string GradeLetter { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }
    }

    //public class AttendanceSummaryDto
    //{
    //    public int TotalClasses { get; set; }
    //    public int Present { get; set; }
    //    public int Absent { get; set; }
    //    public int Late { get; set; }
    //    public int Excused { get; set; }
    //    public decimal AttendancePercentage { get; set; }
    //}
}
