namespace SchoolManagementSystem.Application.DTOs
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public decimal Percentage { get; set; }
        public string GradeLetter { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class CreateGradeDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int EnrollmentId { get; set; }
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class UpdateGradeDto
    {
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class StudentCourseGradeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<GradeDto> Grades { get; set; } = new();
        public decimal AverageScore { get; set; }
        public string FinalGrade { get; set; } = string.Empty;
        public decimal GPA { get; set; }
    }

    public class BulkCreateGradesDto
    {
        public int CourseId { get; set; }
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentName { get; set; } = string.Empty;
        public decimal TotalScore { get; set; }
        public DateTime AssessmentDate { get; set; }
        public List<BulkGradeItemDto> Grades { get; set; } = new();
    }

    public class BulkGradeItemDto
    {
        public int StudentId { get; set; }
        public decimal Score { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}