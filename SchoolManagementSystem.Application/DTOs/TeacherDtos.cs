namespace SchoolManagementSystem.Application.DTOs
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }

    public class TeacherDetailDto : TeacherDto
    {
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTeacherDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string? EmployeeId { get; set; }
    }

    public class UpdateTeacherDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

    public class TeacherStatsDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAssignments { get; set; }
        public decimal AverageStudentRating { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}