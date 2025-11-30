using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<CourseService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<CourseDto> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                throw new NotFoundException(nameof(Course), id);
            }

            return _mapper.Map<CourseDto>(course);
        }
        public async Task<CourseDetailDto> GetCourseDetailAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Enrollments)
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                throw new NotFoundException(nameof(Course), id);
            }

            var courseDetail = _mapper.Map<CourseDetailDto>(course);

            courseDetail.TotalStudents = course.Enrollments.Count;
            courseDetail.TotalClasses = course.Classes.Count;

            return courseDetail;
        }
        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
        {
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == createCourseDto.Code);

            if (existingCourse != null)
            {
                throw new BadRequestException("A course with this code already exists.");
            }

            var course = _mapper.Map<Course>(createCourseDto);
            course.CreatedDate = DateTime.UtcNow;
            course.IsActive = true;

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Course created with ID: {CourseId}", course.Id);

            return await GetCourseByIdAsync(course.Id);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), id);
            }

            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == updateCourseDto.Code && c.Id != id);

            if (existingCourse != null)
            {
                throw new BadRequestException("A course with this code already exists.");
            }

            _mapper.Map(updateCourseDto, course);
            course.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Course updated with ID: {CourseId}", id);

            return await GetCourseByIdAsync(id);
        }
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), id);
            }

            course.IsActive = false;
            course.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Course deleted with ID: {CourseId}", id);
            return true;
        }
        public async Task<bool> AssignTeacherToCourseAsync(int courseId, int teacherId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), courseId);
            }

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), teacherId);
            }

            course.TeacherId = teacherId;
            course.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Teacher {TeacherId} assigned to course {CourseId}", teacherId, courseId);
            return true;
        }

        public async Task<bool> RemoveTeacherFromCourseAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), courseId);
            }

            course.TeacherId = null;
            course.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Teacher removed from course {CourseId}", courseId);
            return true;
        }
        public async Task<APIResponseDto<CourseDto>> GetAllCoursesAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c => c.IsActive == true)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.Search) ||
                    c.Code.Contains(request.Search) ||
                    c.Description.Contains(request.Search) ||
                    (c.Teacher != null && c.Teacher.User.FirstName.Contains(request.Search)) ||
                    (c.Teacher != null && c.Teacher.User.LastName.Contains(request.Search)));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
                "credits" => request.SortDescending ? query.OrderByDescending(c => c.Credits) : query.OrderBy(c => c.Credits),
                "teacher" => request.SortDescending ? query.OrderByDescending(c => c.Teacher.User.LastName) : query.OrderBy(c => c.Teacher.User.LastName),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();

            var courses = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);

            return new APIResponseDto<CourseDto>(courseDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId, SearchRequestDto request, string baseUrl)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new NotFoundException(nameof(Course), courseId);

            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Class)
                .Where(e => e.CourseId == courseId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Student.User.FirstName.Contains(request.Search) ||
                    e.Student.User.LastName.Contains(request.Search) ||
                    e.Student.StudentId.Contains(request.Search) ||
                    e.Class.Name.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(e => e.Student.User.LastName) : query.OrderBy(e => e.Student.User.LastName),
                "class" => request.SortDescending ? query.OrderByDescending(e => e.Class.Name) : query.OrderBy(e => e.Class.Name),
                "date" => request.SortDescending ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
                "status" => request.SortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => query.OrderBy(e => e.Student.User.LastName)
            };

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var enrollmentDtos = _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);

            return new APIResponseDto<EnrollmentDto>(enrollmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

    }

}