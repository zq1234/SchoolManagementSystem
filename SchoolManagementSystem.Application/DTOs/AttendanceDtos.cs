namespace SchoolManagementSystem.Application.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Excused { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    public class BulkCreateAttendanceDto
    {
        public int ClassId { get; set; }
        public DateTime Date { get; set; }
        public List<BulkAttendanceItemDto> Attendances { get; set; } = new();
    }

    public class BulkAttendanceItemDto
    {
        public int StudentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AttendanceReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StudentAttendanceSummaryDto> StudentSummaries { get; set; } = new();
        public ClassAttendanceSummaryDto ClassSummary { get; set; } = new();
    }

    public class StudentAttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Excused { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    public class ClassAttendanceSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public decimal AverageAttendance { get; set; }
        public int PerfectAttendance { get; set; }
        public int LowAttendance { get; set; }
    }
}