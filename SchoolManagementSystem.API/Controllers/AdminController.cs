using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ICourseService _courseService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            IDepartmentService departmentService,
            ICourseService courseService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _departmentService = departmentService;
            _courseService = courseService;
            _logger = logger;
        }

        //  Users Management coverd in the user controller
        //[HttpGet("users")]
        //[ProducesResponseType(typeof(APIResponseDto<UserDto>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetUsers([FromQuery] SearchRequestDto dto)
        //{
        //    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        //    var users = await _userService.GetAllUsersAsync(dto, baseUrl);
        //    return Ok(users);
        //}

        //  Departments Management
        [HttpGet("departments")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<DepartmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepartments([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var departments = await _departmentService.GetAllAsync(request, baseUrl);
            return Ok(departments);
           // return Ok(new ApiResponse<APIResponseDto<DepartmentDto>>(departments, "Departments retrieved successfully"));
        }

        [HttpPost("departments")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            var department = await _departmentService.CreateAsync(createDepartmentDto);
            return CreatedAtAction(
                nameof(GetDepartmentById),
                new { id = department.Id },
                new ApiResponse<DepartmentDto>(department, "Department created successfully")

            );
        }

        [HttpGet("departments/{id}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            return Ok(new ApiResponse<DepartmentDto>(department, "Department retrieved successfully"));
        }

        //  Courses Management
        [HttpGet("courses")]
        [ProducesResponseType(typeof(ApiResponse<APIResponseDto<CourseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCourses([FromQuery] SearchRequestDto request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var courses = await _courseService.GetAllCoursesAsync(request, baseUrl);
            return Ok(new ApiResponse<APIResponseDto<CourseDto>>(courses, "Courses retrieved successfully"));
        }

        [HttpPost("courses")]
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

        [HttpGet("courses/{id}")]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(new ApiResponse<CourseDto>(course, "Course retrieved successfully"));
        }
    }
}