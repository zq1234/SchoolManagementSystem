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
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly ILogger<GradesController> _logger;

        public GradesController(
            IGradeService gradeService,
            ILogger<GradesController> logger)
        {
            _gradeService = gradeService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradeById(int id)
        {
            var grade = await _gradeService.GetGradeByIdAsync(id);
            return Ok(new ApiResponse<GradeDto>(grade, "Grade retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGrades([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var grades = await _gradeService.GetAllGradesAsync(request, baseUrl);
            return Ok(grades);
            //return Ok(new ApiResponse<APIResponseDto<GradeDto>>(grades, "Grades retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGrade([FromBody] CreateGradeDto createGradeDto)
        {
            var grade = await _gradeService.CreateGradeAsync(createGradeDto);
            return CreatedAtAction(
                nameof(GetGradeById),
                new { id = grade.Id },
                new ApiResponse<GradeDto>(grade, "Grade created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeDto updateGradeDto)
        {
            var grade = await _gradeService.UpdateGradeAsync(id, updateGradeDto);
            return Ok(new ApiResponse<GradeDto>(grade, "Grade updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var result = await _gradeService.DeleteGradeAsync(id);
            return Ok(new ApiResponse<bool>(result, "Grade deleted successfully"));
        }

        [HttpGet("student/{studentId}/course/{courseId}")]
        [ProducesResponseType(typeof(ApiResponse<StudentCourseGradeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentCourseGrades(int studentId, int courseId)
        {
            var grades = await _gradeService.GetStudentCourseGradesAsync(studentId, courseId);
            return Ok(new ApiResponse<StudentCourseGradeDto>(grades, "Student course grades retrieved successfully"));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkCreateGrades([FromBody] BulkCreateGradesDto bulkCreateGradesDto)
        {
            var result = await _gradeService.BulkCreateGradesAsync(bulkCreateGradesDto);
            return Ok(new ApiResponse<bool>(result, "Bulk grades created successfully"));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetGradesByStudent(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var grades = await _gradeService.GetGradesByStudentAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<GradeDto>>(grades, "Student grades retrieved successfully"));
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetGradesByCourse(int courseId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var grades = await _gradeService.GetGradesByCourseAsync(courseId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<GradeDto>>(grades, "Course grades retrieved successfully"));
        }
    }
}