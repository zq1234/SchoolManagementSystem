namespace SchoolManagementSystem.Application.DTOs
{
    public class StudentReportDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public List<CourseReportItemDto> Courses { get; set; } = new();
        public decimal OverallGPA { get; set; }
        public decimal OverallAttendance { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class TeacherReportDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<CourseReportItemDto> Courses { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CourseReportDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public int TotalStudents { get; set; }
        public decimal AverageGrade { get; set; }
        public decimal AverageAttendance { get; set; }
        public List<StudentGradeDto> StudentGrades { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class ClassReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public int TotalStudents { get; set; }
        public List<StudentAttendanceDto> StudentAttendances { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class SchoolReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public decimal OverallAttendance { get; set; }
        public decimal OverallGPA { get; set; }
        public List<CourseSummaryDto> TopCourses { get; set; } = new();
        public List<TeacherSummaryDto> TopTeachers { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class FinancialReportDto
    {
        public int AcademicYear { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public List<RevenueItemDto> RevenueItems { get; set; } = new();
        public List<ExpenseItemDto> ExpenseItems { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class CourseReportItemDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public string GradeLetter { get; set; } = string.Empty;
        public decimal Attendance { get; set; }
    }

    public class StudentGradeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public string GradeLetter { get; set; } = string.Empty;
    }

    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal AttendancePercentage { get; set; }
    }

    public class CourseSummaryDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal AverageGrade { get; set; }
    }

    public class TeacherSummaryDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class RevenueItemDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ExpenseItemDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}