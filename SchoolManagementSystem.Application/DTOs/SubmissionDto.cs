using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.DTOs
{
    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public int? GradedByTeacherId { get; set; }
        public string? GradedByTeacherName { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsGraded => !string.IsNullOrEmpty(Grade);
        public TimeSpan TimeUntilDue { get; set; }
    }

    public class CreateSubmissionDto
    {
        public int AssignmentId { get; set; }

        public IFormFile File { get; set; } = null!;

        public string? Remarks { get; set; }
    }

    public class UpdateSubmissionDto
    {
        public string? Remarks { get; set; }
    }

    public class GradeSubmissionDto
    {
        public string Grade { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }

    public class SubmissionStatsDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
        public decimal SubmissionRate { get; set; }
        public int LateSubmissions { get; set; }
        public int OnTimeSubmissions { get; set; }
    }

    public class SubmissionSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int SubmittedAssignments { get; set; }
        public int GradedAssignments { get; set; }
        public int LateSubmissions { get; set; }
        public decimal SubmissionRate { get; set; }
        public decimal AverageGrade { get; set; }
    }
}
