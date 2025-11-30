
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Entities
{
    public class Submission : BaseEntity
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public int? GradedByTeacherId { get; set; }
        public string? Remarks { get; set; }
        public Assignment Assignment { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Teacher? GradedByTeacher { get; set; }
    }
}
