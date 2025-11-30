using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("student/{studentId}")]
        [ProducesResponseType(typeof(ApiResponse<StudentReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateStudentReport(int studentId)
        {
            var report = await _reportService.GenerateStudentReportAsync(studentId);
            return Ok(new ApiResponse<StudentReportDto>(report, "Student report generated successfully"));
        }

        [HttpGet("teacher/{teacherId}")]
        [ProducesResponseType(typeof(ApiResponse<TeacherReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateTeacherReport(int teacherId)
        {
            var report = await _reportService.GenerateTeacherReportAsync(teacherId);
            return Ok(new ApiResponse<TeacherReportDto>(report, "Teacher report generated successfully"));
        }

        [HttpGet("course/{courseId}")]
        [ProducesResponseType(typeof(ApiResponse<CourseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateCourseReport(int courseId)
        {
            var report = await _reportService.GenerateCourseReportAsync(courseId);
            return Ok(new ApiResponse<CourseReportDto>(report, "Course report generated successfully"));
        }

        [HttpGet("class/{classId}")]
        [ProducesResponseType(typeof(ApiResponse<ClassReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateClassReport(int classId)
        {
            var report = await _reportService.GenerateClassReportAsync(classId);
            return Ok(new ApiResponse<ClassReportDto>(report, "Class report generated successfully"));
        }

        [HttpGet("school")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SchoolReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateSchoolReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportService.GenerateSchoolReportAsync(startDate, endDate);
            return Ok(new ApiResponse<SchoolReportDto>(report, "School report generated successfully"));
        }

        [HttpGet("financial/{academicYear}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<FinancialReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateFinancialReport(int academicYear)
        {
            var report = await _reportService.GenerateFinancialReportAsync(academicYear);
            return Ok(new ApiResponse<FinancialReportDto>(report, "Financial report generated successfully"));
        }
    }
}