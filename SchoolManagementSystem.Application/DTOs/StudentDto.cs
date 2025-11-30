using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Application.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudentDetailDto : StudentDto
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public decimal GPA { get; set; }
        public decimal AttendancePercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStudentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? StudentId { get; set; }
    }

    public class UpdateStudentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class StudentStatsDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int ActiveCourses { get; set; }
        public decimal OverallGPA { get; set; }
        public decimal AttendancePercentage { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public DateTime? LastLogin { get; set; }
    }
    public class StudentPhotoDto
    {
        public int StudentId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}