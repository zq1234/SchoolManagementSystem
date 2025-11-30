namespace SchoolManagementSystem.Application.DTOs
{
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public string Room { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int CourseId { get; set; }
        public int? TeacherId { get; set; }
    }

    public class ClassDetailDto : ClassDto
    {
        public int CourseId { get; set; }
        public int? TeacherId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int? TeacherId { get; set; }
        public string Room { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
    }

    public class UpdateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
    }
}