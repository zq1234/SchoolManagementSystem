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
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            ICourseService courseService,
            ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(new ApiResponse<CourseDto>(course, "Course retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var courses = await _courseService.GetAllCoursesAsync(request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<CourseDto>>(courses, "Courses retrieved successfully"));
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<CourseDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseDetail(int id)
        {
            var courseDetail = await _courseService.GetCourseDetailAsync(id);
            return Ok(new ApiResponse<CourseDetailDto>(courseDetail, "Course detail retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
        {
            var course = await _courseService.CreateCourseAsync(createCourseDto);
            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = course.Id },
                new ApiResponse<CourseDto>(course, "Course created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            var course = await _courseService.UpdateCourseAsync(id, updateCourseDto);
            return Ok(new ApiResponse<CourseDto>(course, "Course updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return Ok(new ApiResponse<bool>(result, "Course deleted successfully"));
        }

        [HttpGet("{courseId}/enrollments")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _courseService.GetCourseEnrollmentsAsync(courseId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Course enrollments retrieved successfully"));
        }

        [HttpPost("{courseId}/assign-teacher/{teacherId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignTeacherToCourse(int courseId, int teacherId)
        {
            var result = await _courseService.AssignTeacherToCourseAsync(courseId, teacherId);
            return Ok(new ApiResponse<bool>(result, "Teacher assigned to course successfully"));
        }

        [HttpDelete("{courseId}/remove-teacher")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveTeacherFromCourse(int courseId)
        {
            var result = await _courseService.RemoveTeacherFromCourseAsync(courseId);
            return Ok(new ApiResponse<bool>(result, "Teacher removed from course successfully"));
        }
    }
}