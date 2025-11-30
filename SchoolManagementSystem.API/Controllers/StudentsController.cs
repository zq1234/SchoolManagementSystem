using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.API.Extensions;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Exceptions;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IStudentService studentService,
            ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            return Ok(new ApiResponse<StudentDto>(student, "Student retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var students = await _studentService.GetPagedStudentsAsync(request, baseUrl);
            return Ok(students);
            //return Ok(new ApiResponse<APIResponseDto<StudentDto>>(students, "Students retrieved successfully"));
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<StudentDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStudentDetail(int id)
        {
            var studentDetail = await _studentService.GetStudentDetailAsync(id);
            return Ok(new ApiResponse<StudentDetailDto>(studentDetail, "Student detail retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            var student = await _studentService.CreateStudentAsync(createStudentDto);
            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = student.Id },
                new ApiResponse<StudentDto>(student, "Student created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            var student = await _studentService.UpdateStudentAsync(id, updateStudentDto);
            return Ok(new ApiResponse<StudentDto>(student, "Student updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            return Ok(new ApiResponse<bool>(result, "Student deleted successfully"));
        }

        [HttpGet("{studentId}/enrollments")]
        public async Task<IActionResult> GetStudentEnrollments(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _studentService.GetStudentEnrollmentsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Student enrollments retrieved successfully"));
        }

        [HttpGet("{studentId}/grades")]
        public async Task<IActionResult> GetStudentGrades(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var grades = await _studentService.GetStudentGradesAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<GradeDto>>(grades, "Student grades retrieved successfully"));
        }

        [HttpGet("{studentId}/attendance")]
        public async Task<IActionResult> GetStudentAttendance(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendance = await _studentService.GetStudentAttendanceAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendance, "Student attendance retrieved successfully"));
        }

        [HttpGet("{studentId}/stats")]
        [ProducesResponseType(typeof(ApiResponse<StudentStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentStats(int studentId)
        {
            var stats = await _studentService.GetStudentStatsAsync(studentId);
            return Ok(new ApiResponse<StudentStatsDto>(stats, "Student stats retrieved successfully"));
        }

        [HttpPost("upload-photo")]
        [Authorize(Roles = "Admin,Student")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadStudentPhoto([FromBody] StudentPhotoDto dto)
        {
            var result = await _studentService.UploadStudentPhotoAsync(dto);
            return Ok(new ApiResponse<bool>(result, "Student photo uploaded successfully"));
        }

        [HttpGet("{studentId}/assignments")]
        public async Task<IActionResult> GetStudentAssignments(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _studentService.GetStudentAssignmentsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Student assignments retrieved successfully"));
        }

        //[HttpPost("submit-assignment")]
        //[Authorize(Roles = "Student")]
        //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> SubmitAssignment([FromBody] SubmitAssignmentDto dto)
        //{
        //    var result = await _studentService.SubmitAssignmentAsync(dto);
        //    return Ok(new ApiResponse<bool>(result, "Assignment submitted successfully"));
        //}

        [HttpGet("{studentId}/notifications")]
        public async Task<IActionResult> GetStudentNotifications(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var notifications = await _studentService.GetStudentNotificationsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<NotificationDto>>(notifications, "Student notifications retrieved successfully"));
        }
        [HttpGet("my-profile")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<StudentDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfile()
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var studentDetail = await _studentService.GetStudentDetailAsync(studentId);
            return Ok(new ApiResponse<StudentDetailDto>(studentDetail, "Your profile retrieved successfully"));
        }

        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<EnrollmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] SearchRequestDto request)
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _studentService.GetStudentEnrollmentsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Your enrollments retrieved successfully"));
        }

        [HttpGet("my-grades")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<GradeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyGrades([FromQuery] SearchRequestDto request)
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var grades = await _studentService.GetStudentGradesAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<GradeDto>>(grades, "Your grades retrieved successfully"));
        }

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<AttendanceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyAttendance([FromQuery] SearchRequestDto request)
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendance = await _studentService.GetStudentAttendanceAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendance, "Your attendance retrieved successfully"));
        }

        [HttpGet("my-assignments")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<AssignmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyAssignments([FromQuery] SearchRequestDto request)
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _studentService.GetStudentAssignmentsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Your assignments retrieved successfully"));
        }

        [HttpGet("my-notifications")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] SearchRequestDto request)
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var notifications = await _studentService.GetStudentNotificationsAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<NotificationDto>>(notifications, "Your notifications retrieved successfully"));
        }

        [HttpGet("my-stats")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<StudentStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStats()
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var stats = await _studentService.GetStudentStatsAsync(studentId);
            return Ok(new ApiResponse<StudentStatsDto>(stats, "Your statistics retrieved successfully"));
        }

        [HttpGet("my-dashboard")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<StudentDashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyDashboard()
        {
            var studentId = User.GetStudentId();
            if (studentId == 0)
                throw new UnauthorizedException("Student ID not found in token");

            var dashboard = await _studentService.GetStudentDashboardAsync(studentId);
            return Ok(new ApiResponse<StudentDashboardDto>(dashboard, "Your dashboard retrieved successfully"));
        }

        [HttpGet("{studentId}/dashboard")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<StudentDashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentDashboard(int studentId)
        {
            var dashboard = await _studentService.GetStudentDashboardAsync(studentId);
            return Ok(new ApiResponse<StudentDashboardDto>(dashboard, "Student dashboard retrieved successfully"));
        }




            
            [HttpGet("my-classes")]
            [Authorize(Roles = "Student")]
            [ProducesResponseType(typeof(ApiResponse<APIResponseDto<ClassDto>>), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetMyClasses([FromQuery] SearchRequestDto request)
            {
                var studentId = User.GetStudentId();
                if (studentId == 0)
                    throw new UnauthorizedException("Student ID not found in token");

                // Get classes through enrollments
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
                var enrollments = await _studentService.GetStudentEnrollmentsAsync(studentId, request, baseUrl);

                // Transform enrollments to classes
                var classes = new APIResponseDto<ClassDto>(
                    enrollments.Data.Select(e => new ClassDto
                    {
                        Id = e.ClassId,
                        Name = e.ClassName,
                        CourseName = e.CourseName
                        // Add other properties as needed
                    }),
                    enrollments.Page,
                    enrollments.PageSize,
                    enrollments.TotalCount,
                    baseUrl
                );

                return Ok(new ApiResponse<APIResponseDto<ClassDto>>(classes, "Your classes retrieved successfully"));
            }
}
}
