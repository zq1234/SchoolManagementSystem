
using System.Security.Claims;

namespace SchoolManagementSystem.Core.Entities
{
    public class Teacher : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}