using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.API.Extensions;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Exceptions;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(
            ITeacherService teacherService,
            ILogger<TeachersController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            return Ok(new ApiResponse<TeacherDto>(teacher, "Teacher retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeachers([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var teachers = await _teacherService.GetAllTeachersAsync(request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<TeacherDto>>(teachers, "Teachers retrieved successfully"));
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<TeacherDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacherDetail(int id)
        {
            var teacherDetail = await _teacherService.GetTeacherDetailAsync(id);
            return Ok(new ApiResponse<TeacherDetailDto>(teacherDetail, "Teacher detail retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto createTeacherDto)
        {
            var teacher = await _teacherService.CreateTeacherAsync(createTeacherDto);
            return CreatedAtAction(
                nameof(GetTeacherById),
                new { id = teacher.Id },
                new ApiResponse<TeacherDto>(teacher, "Teacher created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            var teacher = await _teacherService.UpdateTeacherAsync(id, updateTeacherDto);
            return Ok(new ApiResponse<TeacherDto>(teacher, "Teacher updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);
            return Ok(new ApiResponse<bool>(result, "Teacher deleted successfully"));
        }

        [HttpGet("{teacherId}/stats")]
        [ProducesResponseType(typeof(ApiResponse<TeacherStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacherStats(int teacherId)
        {
            var stats = await _teacherService.GetTeacherStatsAsync(teacherId);
            return Ok(new ApiResponse<TeacherStatsDto>(stats, "Teacher stats retrieved successfully"));
        }

        [HttpGet("{teacherId}/courses")]
        public async Task<IActionResult> GetTeacherCourses(int teacherId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var courses = await _teacherService.GetTeacherCoursesAsync(teacherId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<CourseDto>>(courses, "Teacher courses retrieved successfully"));
        }

        [HttpGet("{teacherId}/classes")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherClasses(int teacherId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var classes = await _teacherService.GetTeacherClassesAsync(teacherId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<ClassDto>>(classes, "Teacher classes retrieved successfully"));
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<TeacherDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var teacherDetail = await _teacherService.GetTeacherDetailAsync(teacherId);
            return Ok(new ApiResponse<TeacherDetailDto>(teacherDetail, "Your profile retrieved successfully"));
        }

        [HttpPost("classes/{classId}/enroll-student/{studentId}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnrollStudentInClass(int classId, int studentId)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var result = await _teacherService.EnrollStudentInClassAsync(classId, studentId, teacherId);
            return Ok(new ApiResponse<bool>(result, "Student enrolled in class successfully"));
        }

        [HttpDelete("classes/{classId}/remove-student/{studentId}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveStudentFromClass(int classId, int studentId)
        {
            var result = await _teacherService.RemoveStudentFromClassAsync(classId, studentId);
            return Ok(new ApiResponse<bool>(result, "Student removed from class successfully"));
        }

        [HttpGet("classes/{classId}/students")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<StudentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassStudents(int classId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var students = await _teacherService.GetClassStudentsAsync(classId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<StudentDto>>(students, "Class students retrieved successfully"));
        }

        [HttpPost("classes/{classId}/bulk-enroll")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<BulkEnrollmentResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkEnrollStudents(int classId, [FromBody] BulkEnrollmentRequestDto request)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var result = await _teacherService.BulkEnrollStudentsAsync(classId, request.StudentIds, teacherId);
            return Ok(new ApiResponse<BulkEnrollmentResultDto>(result, "Bulk enrollment completed successfully"));
        }

        [HttpGet("my-stats")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<TeacherStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStats()
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var stats = await _teacherService.GetTeacherStatsAsync(teacherId);
            return Ok(new ApiResponse<TeacherStatsDto>(stats, "Your statistics retrieved successfully"));
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<CourseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCourses([FromQuery] SearchRequestDto request)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var courses = await _teacherService.GetTeacherCoursesAsync(teacherId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<CourseDto>>(courses, "Your courses retrieved successfully"));
        }

        [HttpGet("my-classes")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<ClassDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyClasses([FromQuery] SearchRequestDto request)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var classes = await _teacherService.GetTeacherClassesAsync(teacherId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<ClassDto>>(classes, "Your classes retrieved successfully"));
        }



        [HttpPost("classes")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassDto createClassDto)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            createClassDto.TeacherId = teacherId;
            var classObj = await _teacherService.CreateClassAsync(createClassDto);

            return CreatedAtAction(
                nameof(GetClassById),
                new { id = classObj.Id },
                new ApiResponse<ClassDto>(classObj, "Class created successfully")
            );
        }
        [HttpGet("classes/{id}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassById(int id)
        {
            var teacherId = User.GetTeacherId();
            if (teacherId == 0)
                throw new UnauthorizedException("Teacher ID not found in token");

            // Add authorization to ensure teacher can only access their own classes
            var classObj = await _teacherService.GetClassByIdAsync(id);

            // Verify the class belongs to this teacher
            if (classObj.TeacherId != teacherId)
                throw new UnauthorizedException("You are not authorized to access this class.");

            return Ok(new ApiResponse<ClassDto>(classObj, "Class retrieved successfully"));
        }
        [HttpPut("classes/{id}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] UpdateClassDto updateClassDto)
        {
            var teacherId = User.GetTeacherId();
            // Add authorization to ensure teacher can only update their own classes
            var classObj = await _teacherService.UpdateClassAsync(id, updateClassDto);
            return Ok(new ApiResponse<ClassDto>(classObj, "Class updated successfully"));
        }

        [HttpPatch("classes/{id}/deactivate")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateClass(int id)
        {
            var teacherId = User.GetTeacherId();
            // Add authorization logic
            var result = await _teacherService.DeactivateClassAsync(id);
            return Ok(new ApiResponse<bool>(result, "Class deactivated successfully"));
        }

        [HttpGet("attendance/{classId}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<AttendanceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceHistory(int classId, [FromQuery] SearchRequestDto request)
        {
            var teacherId = User.GetTeacherId();
            // Add authorization to ensure teacher can only access their class attendance
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendance = await _teacherService.GetClassAttendanceHistoryAsync(classId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendance, "Attendance history retrieved successfully"));
        }

        [HttpGet("assignments/{classId}")]
        [Authorize(Roles = "Teacher")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<AssignmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassAssignments(int classId, [FromQuery] SearchRequestDto request)
        {
            var teacherId = User.GetTeacherId();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _teacherService.GetClassAssignmentsAsync(classId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Class assignments retrieved successfully"));
        }
    }


}