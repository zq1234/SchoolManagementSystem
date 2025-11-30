using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
    public class StudentClassDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ClassSection { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateStudentClassDto
    {
        public int StudentId { get; set; }

        public int ClassId { get; set; }
    }

    public class StudentClassSummaryDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public DateTime LastEnrollment { get; set; }
    }

    public class StudentEnrollmentSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public DateTime FirstEnrollment { get; set; }
        public DateTime LastEnrollment { get; set; }
    }

    public class BulkEnrollmentDto
    {
        public int ClassId { get; set; }

        public List<int> StudentIds { get; set; } = new();
    }

    public class EnrollmentStatsDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int CurrentEnrollment { get; set; }
        public int AvailableSlots => TotalCapacity - CurrentEnrollment;
        public decimal EnrollmentRate => TotalCapacity > 0 ? (decimal)CurrentEnrollment / TotalCapacity * 100 : 0;
    }
}
