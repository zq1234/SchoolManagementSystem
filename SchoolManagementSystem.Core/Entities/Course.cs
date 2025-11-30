
using System.Security.Claims;

namespace SchoolManagementSystem.Core.Entities
{
    public class Course : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int Duration { get; set; }
        public decimal Fee { get; set; }
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}