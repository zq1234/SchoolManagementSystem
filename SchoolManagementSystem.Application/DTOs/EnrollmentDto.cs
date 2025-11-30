namespace SchoolManagementSystem.Application.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
    }

    public class CreateEnrollmentDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
    }

    public class UpdateEnrollmentDto
    {
        public int ClassId { get; set; }
    }

    public class EnrollmentStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
    }
}