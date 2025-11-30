using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    //[Authorize]
    //public class StudentClassesController : ControllerBase
    //{
    //    private readonly IStudentClassService _studentClassService;
    //    private readonly ILogger<StudentClassesController> _logger;

    //    public StudentClassesController(
    //        IStudentClassService studentClassService,
    //        ILogger<StudentClassesController> logger)
    //    {
    //        _studentClassService = studentClassService;
    //        _logger = logger;
    //    }

    //    [HttpGet("{id}")]
    //    [ProducesResponseType(typeof(ApiResponse<StudentClassDto>), StatusCodes.Status200OK)]
    //    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    //    public async Task<IActionResult> GetStudentClassById(int id)
    //    {
    //        var studentClass = await _studentClassService.GetStudentClassByIdAsync(id);
    //        return Ok(new ApiResponse<StudentClassDto>(studentClass, "Student class retrieved successfully"));
    //    }

    //    [HttpPost("enroll")]
    //    [Authorize(Roles = "Admin")]
    //    [ProducesResponseType(typeof(ApiResponse<StudentClassDto>), StatusCodes.Status201Created)]
    //    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    //    public async Task<IActionResult> EnrollStudent([FromBody] CreateStudentClassDto createStudentClassDto)
    //    {
    //        var studentClass = await _studentClassService.EnrollStudentAsync(createStudentClassDto);
    //        return CreatedAtAction(
    //            nameof(GetStudentClassById),
    //            new { id = studentClass.Id },
    //            new ApiResponse<StudentClassDto>(studentClass, "Student enrolled successfully")
    //        );
    //    }

    //    [HttpDelete("student/{studentId}/class/{classId}")]
    //    [Authorize(Roles = "Admin")]
    //    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    //    public async Task<IActionResult> RemoveStudentFromClass(int studentId, int classId)
    //    {
    //        var result = await _studentClassService.RemoveStudentFromClassAsync(studentId, classId);
    //        return Ok(new ApiResponse<bool>(result, "Student removed from class successfully"));
    //    }

    //    [HttpGet("class/{classId}/count")]
    //    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    //    public async Task<IActionResult> GetClassStudentCount(int classId)
    //    {
    //        var count = await _studentClassService.GetClassStudentCountAsync(classId);
    //        return Ok(new ApiResponse<int>(count, "Class student count retrieved successfully"));
    //    }

    //    [HttpGet("student/{studentId}/class/{classId}/enrolled")]
    //    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //    public async Task<IActionResult> IsStudentEnrolled(int studentId, int classId)
    //    {
    //        var isEnrolled = await _studentClassService.IsStudentEnrolledAsync(studentId, classId);
    //        return Ok(new ApiResponse<bool>(isEnrolled, "Student enrollment status retrieved successfully"));
    //    }

    //    [HttpGet("class/{classId}/enrollments")]
    //    public async Task<IActionResult> GetClassEnrollments(int classId, [FromQuery] SearchRequestDto request)
    //    {
    //        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
    //        var enrollments = await _studentClassService.GetClassEnrollmentsAsync(classId, request, baseUrl);
    //        return Ok(new ApiResponse<APIResponseDto<StudentClassDto>>(enrollments, "Class enrollments retrieved successfully"));
    //    }

    //    [HttpGet("student/{studentId}/enrollments")]
    //    public async Task<IActionResult> GetStudentEnrollments(int studentId, [FromQuery] SearchRequestDto request)
    //    {
    //        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
    //        var enrollments = await _studentClassService.GetStudentEnrollmentsAsync(studentId, request, baseUrl);
    //        return Ok(new ApiResponse<APIResponseDto<StudentClassDto>>(enrollments, "Student enrollments retrieved successfully"));
    //    }
    //}
}