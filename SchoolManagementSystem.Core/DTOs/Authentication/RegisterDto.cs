namespace SchoolManagementSystem.Core.DTOs.Authentication
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public string? StudentId { get; set; }
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Qualification { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}