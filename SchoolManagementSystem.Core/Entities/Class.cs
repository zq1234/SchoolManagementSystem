

namespace SchoolManagementSystem.Core.Entities
{
    public class Class : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty; 
        public string Semester { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public int? TeacherId { get; set; }  
        public string Room { get; set; } = string.Empty;  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;  
        public Course? Course { get; set; } = null!;
        public Teacher? Teacher { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}