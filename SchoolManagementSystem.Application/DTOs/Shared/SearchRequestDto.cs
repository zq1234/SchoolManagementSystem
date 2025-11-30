using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace SchoolManagementSystem.Application.DTOs.Shared
{
    public class SearchRequestDto
    {
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
        public string? SortBy { get; set; }

        public bool SortDescending { get; set; } = false;

        // Helper properties for EF/LINQ

        internal int Skip => (Page - 1) * PageSize;
        internal int Take => PageSize;
        internal bool HasSearch => !string.IsNullOrWhiteSpace(Search);
        internal bool HasSorting => !string.IsNullOrWhiteSpace(SortBy);
       
        //public int Skip => (Page - 1) * PageSize;
        //public int Take => PageSize;
        //public bool HasSearch => !string.IsNullOrWhiteSpace(Search);
        //public bool HasSorting => !string.IsNullOrWhiteSpace(SortBy);
    }

    public class DateRangeRequestDto
    {
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool HasDateRange => StartDate.HasValue && EndDate.HasValue;

        // Validation method
        public bool IsValidDateRange()
        {
            return HasDateRange && StartDate <= EndDate;
        }
    }

    public class AssignmentFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be greater than 0")]
        public int? ClassId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be greater than 0")]
        public int? CourseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be greater than 0")]
        public int? TeacherId { get; set; }

        public bool? IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime? DueDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDateTo { get; set; }

        public bool HasDueDateRange => DueDateFrom.HasValue && DueDateTo.HasValue;
    }

    public class SubmissionFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Assignment ID must be greater than 0")]
        public int? AssignmentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Student ID must be greater than 0")]
        public int? StudentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be greater than 0")]
        public int? ClassId { get; set; }

        public bool? IsGraded { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SubmittedDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SubmittedDateTo { get; set; }

        public bool HasSubmittedDateRange => SubmittedDateFrom.HasValue && SubmittedDateTo.HasValue;
    }

    public class NotificationFilterDto : SearchRequestDto
    {
        [StringLength(20, ErrorMessage = "Recipient role cannot exceed 20 characters")]
        public string? RecipientRole { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Recipient ID must be greater than 0")]
        public int? RecipientId { get; set; }

        public bool? IsRead { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CreatedDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CreatedDateTo { get; set; }

        public bool HasCreatedDateRange => CreatedDateFrom.HasValue && CreatedDateTo.HasValue;
    }

    // Additional commonly used filter DTOs
    public class StudentFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be greater than 0")]
        public int? DepartmentId { get; set; }

        [StringLength(20, ErrorMessage = "Enrollment status cannot exceed 20 characters")]
        public string? EnrollmentStatus { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EnrollmentDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EnrollmentDateTo { get; set; }

        public bool HasEnrollmentDateRange => EnrollmentDateFrom.HasValue && EnrollmentDateTo.HasValue;
    }

    public class TeacherFilterDto : SearchRequestDto
    {
        [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters")]
        public string? Department { get; set; }

        [StringLength(50, ErrorMessage = "Qualification cannot exceed 50 characters")]
        public string? Qualification { get; set; }

        [DataType(DataType.Date)]
        public DateTime? HireDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? HireDateTo { get; set; }

        public bool HasHireDateRange => HireDateFrom.HasValue && HireDateTo.HasValue;
    }

    public class AttendanceFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Student ID must be greater than 0")]
        public int? StudentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be greater than 0")]
        public int? ClassId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be greater than 0")]
        public int? CourseId { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string? Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        public bool HasDateRange => DateFrom.HasValue && DateTo.HasValue;
    }

    public class CourseFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be greater than 0")]
        public int? DepartmentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be greater than 0")]
        public int? TeacherId { get; set; }

        [Range(0, 10, ErrorMessage = "Credits must be between 0 and 10")]
        public int? Credits { get; set; }

        public bool? IsActive { get; set; } = true;
    }

    public class ClassFilterDto : SearchRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Course ID must be greater than 0")]
        public int? CourseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be greater than 0")]
        public int? TeacherId { get; set; }

        [StringLength(20, ErrorMessage = "Semester cannot exceed 20 characters")]
        public string? Semester { get; set; }

        [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public string? Section { get; set; }

        public bool? IsActive { get; set; } = true;
    }
}

