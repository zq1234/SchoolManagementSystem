
using SchoolManagementSystem.Core.Enums;
using System.Diagnostics;

namespace SchoolManagementSystem.Core.Entities
{
    // alternative to StudentClass
    public class Enrollment : BaseEntity 
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        public DateTime? CompletionDate { get; set; }
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();




    }
}