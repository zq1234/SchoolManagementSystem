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
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(
            IAssignmentService assignmentService,
            ILogger<AssignmentsController> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            return Ok(new ApiResponse<AssignmentDto>(assignment, "Assignment retrieved successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssignments([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _assignmentService.GetAllAssignmentsAsync(request, baseUrl);
            return Ok(assignments);
            //return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Assignments retrieved successfully"));
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignmentDetail(int id)
        {
            var assignmentDetail = await _assignmentService.GetAssignmentDetailAsync(id);
            return Ok(new ApiResponse<AssignmentDetailDto>(assignmentDetail, "Assignment detail retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto createAssignmentDto, [FromQuery] int teacherId)
        {
            var assignment = await _assignmentService.CreateAssignmentAsync(createAssignmentDto, teacherId);
            return CreatedAtAction(
                nameof(GetAssignmentById),
                new { id = assignment.Id },
                new ApiResponse<AssignmentDto>(assignment, "Assignment created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto updateAssignmentDto)
        {
            var assignment = await _assignmentService.UpdateAssignmentAsync(id, updateAssignmentDto);
            return Ok(new ApiResponse<AssignmentDto>(assignment, "Assignment updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var result = await _assignmentService.DeleteAssignmentAsync(id);
            return Ok(new ApiResponse<bool>(result, "Assignment deleted successfully"));
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetAssignmentsByClass(int classId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _assignmentService.GetAssignmentsByClassAsync(classId, request, baseUrl);
            return Ok(assignments);
            //return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Class assignments retrieved successfully"));
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetAssignmentsByTeacher(int teacherId, [FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var assignments = await _assignmentService.GetAssignmentsByTeacherAsync(teacherId, request, baseUrl);
            return Ok(assignments);
            //return Ok(new ApiResponse<APIResponseDto<AssignmentDto>>(assignments, "Teacher assignments retrieved successfully"));
        }
    }
}