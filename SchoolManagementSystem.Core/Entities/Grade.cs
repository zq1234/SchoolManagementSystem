

namespace SchoolManagementSystem.Core.Entities
{
    public class Grade : BaseEntity
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int EnrollmentId { get; set; }
        public string AssessmentType { get; set; } = string.Empty;
        public string AssessmentName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public decimal Percentage => TotalScore > 0 ? (Score / TotalScore) * 100 : 0;
        public string GradeLetter => CalculateGradeLetter(Percentage);
        public DateTime AssessmentDate { get; set; }
        public string Comments { get; set; } = string.Empty;

        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Enrollment Enrollment { get; set; } = null!;

        private static string CalculateGradeLetter(decimal percentage)
        {
            return percentage switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}