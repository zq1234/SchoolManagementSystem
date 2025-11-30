using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Exceptions;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(
            ISubmissionService submissionService,
            ILogger<SubmissionsController> logger)
        {
            _submissionService = submissionService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            return Ok(new ApiResponse<SubmissionDto>(submission, "Submission retrieved successfully"));
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<SubmissionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitAssignment([FromBody] CreateSubmissionDto createSubmissionDto, [FromQuery] int studentId)
        {
            var submission = await _submissionService.SubmitAssignmentAsync(createSubmissionDto, studentId);
            return CreatedAtAction(
                nameof(GetSubmissionById),
                new { id = submission.Id },
                new ApiResponse<SubmissionDto>(submission, "Assignment submitted successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<SubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] UpdateSubmissionDto updateSubmissionDto)
        {
            var submission = await _submissionService.UpdateSubmissionAsync(id, updateSubmissionDto);
            return Ok(new ApiResponse<SubmissionDto>(submission, "Submission updated successfully"));
        }

        [HttpPatch("{id}/grade")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<SubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionDto gradeSubmissionDto, [FromQuery] int teacherId)
        {
            var submission = await _submissionService.GradeSubmissionAsync(id, gradeSubmissionDto, teacherId);
            return Ok(new ApiResponse<SubmissionDto>(submission, "Submission graded successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var result = await _submissionService.DeleteSubmissionAsync(id);
            return Ok(new ApiResponse<bool>(result, "Submission deleted successfully"));
        }

        [HttpGet("assignment/{assignmentId}/stats")]
        [ProducesResponseType(typeof(ApiResponse<SubmissionStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubmissionStats(int assignmentId)
        {
            var stats = await _submissionService.GetSubmissionStatsAsync(assignmentId);
            return Ok(new ApiResponse<SubmissionStatsDto>(stats, "Submission stats retrieved successfully"));
        }

        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetSubmissionsByAssignment(int assignmentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var submissions = await _submissionService.GetSubmissionsByAssignmentAsync(assignmentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<SubmissionDto>>(submissions, "Assignment submissions retrieved successfully"));
        }
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetSubmissionsByStudent(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var submissions = await _submissionService.GetSubmissionsByStudentAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<SubmissionDto>>(submissions, "Student submissions retrieved successfully"));
        }
    }
}