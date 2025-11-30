using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(
            IEnrollmentService enrollmentService,
            ILogger<EnrollmentsController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrollmentById(int id)
        {
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            return Ok(new ApiResponse<EnrollmentDto>(enrollment, "Enrollment retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEnrollments([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync(request, baseUrl);
            return Ok(enrollments);
            //return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Enrollments retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentDto createEnrollmentDto)
        {
            var enrollment = await _enrollmentService.CreateEnrollmentAsync(createEnrollmentDto);
            return CreatedAtAction(
                nameof(GetEnrollmentById),
                new { id = enrollment.Id },
                new ApiResponse<EnrollmentDto>(enrollment, "Enrollment created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentDto updateEnrollmentDto)
        {
            var enrollment = await _enrollmentService.UpdateEnrollmentAsync(id, updateEnrollmentDto);
            return Ok(new ApiResponse<EnrollmentDto>(enrollment, "Enrollment updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var result = await _enrollmentService.DeleteEnrollmentAsync(id);
            return Ok(new ApiResponse<bool>(result, "Enrollment deleted successfully"));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEnrollmentStatus(int id, [FromBody] EnrollmentStatusDto statusDto)
        {
            var result = await _enrollmentService.UpdateEnrollmentStatusAsync(id, statusDto);
            return Ok(new ApiResponse<bool>(result, "Enrollment status updated successfully"));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetEnrollmentsByStudent(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Student enrollments retrieved successfully"));
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetEnrollmentsByCourse(int courseId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Course enrollments retrieved successfully"));
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetEnrollmentsByClass(int classId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _enrollmentService.GetEnrollmentsByClassAsync(classId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Class enrollments retrieved successfully"));
        }
    }
}