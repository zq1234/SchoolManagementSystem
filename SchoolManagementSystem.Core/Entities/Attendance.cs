
using SchoolManagementSystem.Core.Enums;

namespace SchoolManagementSystem.Core.Entities
{
    public class Attendance : BaseEntity
    {
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int MarkedByTeacherId { get; set; } 
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
        public Teacher Teacher { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
    }
}