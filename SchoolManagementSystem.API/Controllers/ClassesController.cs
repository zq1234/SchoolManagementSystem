using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(
            IClassService classService,
            ILogger<ClassesController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassById(int id)
        {
            var classObj = await _classService.GetClassByIdAsync(id);
            return Ok(new ApiResponse<ClassDto>(classObj, "Class retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var classes = await _classService.GetAllClassesAsync(request, baseUrl);
            return Ok(classes);
            //return Ok(new ApiResponse<APIResponseDto<ClassDto>>(classes, "Classes retrieved successfully"));
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ClassDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassDetail(int id)
        {
            var classDetail = await _classService.GetClassDetailAsync(id);
            return Ok(new ApiResponse<ClassDetailDto>(classDetail, "Class detail retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassDto createClassDto)
        {
            var classObj = await _classService.CreateClassAsync(createClassDto);
            return CreatedAtAction(
                nameof(GetClassById),
                new { id = classObj.Id },
                new ApiResponse<ClassDto>(classObj, "Class created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] UpdateClassDto updateClassDto)
        {
            var classObj = await _classService.UpdateClassAsync(id, updateClassDto);
            return Ok(new ApiResponse<ClassDto>(classObj, "Class updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var result = await _classService.DeleteClassAsync(id);
            return Ok(new ApiResponse<bool>(result, "Class deleted successfully"));
        }

        [HttpGet("{classId}/enrollments")]
        public async Task<IActionResult> GetClassEnrollments(int classId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var enrollments = await _classService.GetClassEnrollmentsAsync(classId, request, baseUrl);
            
            return Ok(enrollments);
           // return Ok(new ApiResponse<APIResponseDto<EnrollmentDto>>(enrollments, "Class enrollments retrieved successfully"));
        }

        [HttpPost("{classId}/assign-teacher/{teacherId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignTeacherToClass(int classId, int teacherId)
        {
            var result = await _classService.AssignTeacherToClassAsync(classId, teacherId);
            return Ok(new ApiResponse<bool>(result, "Teacher assigned to class successfully"));
        }

        [HttpDelete("{classId}/remove-teacher")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveTeacherFromClass(int classId)
        {
            var result = await _classService.RemoveTeacherFromClassAsync(classId);
            return Ok(new ApiResponse<bool>(result, "Teacher removed from class successfully"));
        }
    }
}