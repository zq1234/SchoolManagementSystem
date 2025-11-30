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
    public class AttendancesController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendancesController> _logger;

        public AttendancesController(
            IAttendanceService attendanceService,
            ILogger<AttendancesController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            return Ok(new ApiResponse<AttendanceDto>(attendance, "Attendance retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAttendances([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendances = await _attendanceService.GetAllAttendanceAsync(request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendances, "Attendances retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAttendance([FromBody] CreateAttendanceDto createAttendanceDto)
        {
            var attendance = await _attendanceService.CreateAttendanceAsync(createAttendanceDto);
            return CreatedAtAction(
                nameof(GetAttendanceById),
                new { id = attendance.Id },
                new ApiResponse<AttendanceDto>(attendance, "Attendance created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceDto updateAttendanceDto)
        {
            var attendance = await _attendanceService.UpdateAttendanceAsync(id, updateAttendanceDto);
            return Ok(new ApiResponse<AttendanceDto>(attendance, "Attendance updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceService.DeleteAttendanceAsync(id);
            return Ok(new ApiResponse<bool>(result, "Attendance deleted successfully"));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendanceByStudent(int studentId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendances = await _attendanceService.GetAttendanceByStudentAsync(studentId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendances, "Student attendances retrieved successfully"));
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetAttendanceByClass(int classId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendances = await _attendanceService.GetAttendanceByClassAsync(classId, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendances, "Class attendances retrieved successfully"));
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAttendanceByDate(DateTime date, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var attendances = await _attendanceService.GetAttendanceByDateAsync(date, request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<AttendanceDto>>(attendances, "Date attendances retrieved successfully"));
        }

        [HttpGet("student/{studentId}/course/{courseId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceSummary(int studentId, int courseId)
        {
            var summary = await _attendanceService.GetAttendanceSummaryAsync(studentId, courseId);
            return Ok(new ApiResponse<AttendanceSummaryDto>(summary, "Attendance summary retrieved successfully"));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkCreateAttendance([FromBody] BulkCreateAttendanceDto bulkCreateAttendanceDto)
        {
            var result = await _attendanceService.BulkCreateAttendanceAsync(bulkCreateAttendanceDto);
            return Ok(new ApiResponse<bool>(result, "Bulk attendance created successfully"));
        }

        [HttpGet("class/{classId}/report")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateAttendanceReport(int classId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _attendanceService.GenerateAttendanceReportAsync(classId, startDate, endDate);
            return Ok(new ApiResponse<AttendanceReportDto>(report, "Attendance report generated successfully"));
        }
    }
}