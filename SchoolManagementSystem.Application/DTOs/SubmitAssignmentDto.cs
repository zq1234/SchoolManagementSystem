using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Application.DTOs
{
    public class SubmitAssignmentDto
    {
        [Required(ErrorMessage = "Assignment ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Assignment ID must be greater than 0")]
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Student ID must be greater than 0")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }
}