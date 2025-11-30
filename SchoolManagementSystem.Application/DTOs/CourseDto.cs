namespace SchoolManagementSystem.Application.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int Duration { get; set; }
        public decimal Fee { get; set; }
        public string? TeacherName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CourseDetailDto : CourseDto
    {
        public int? TeacherId { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCourseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int Duration { get; set; }
        public decimal Fee { get; set; }
        public int? TeacherId { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class UpdateCourseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int Duration { get; set; }
        public decimal Fee { get; set; }
        public bool IsActive { get; set; }
    }
}