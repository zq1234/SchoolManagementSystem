using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnrollmentService> _logger;
        public EnrollmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<EnrollmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }
        public async Task<EnrollmentDto> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
            {
                throw new NotFoundException(nameof(Enrollment), id);
            }

            return _mapper.Map<EnrollmentDto>(enrollment);
        }
        public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
        {
            // Check if student exists
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == createEnrollmentDto.StudentId);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), createEnrollmentDto.StudentId);
            }

            // Check if course exists
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == createEnrollmentDto.CourseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), createEnrollmentDto.CourseId);
            }

            // Check if class exists and belongs to course
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == createEnrollmentDto.ClassId && c.CourseId == createEnrollmentDto.CourseId);

            if (classEntity == null)
            {
                throw new NotFoundException("Class not found or does not belong to the specified course.");
            }

            // Check if student is already enrolled in this course
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == createEnrollmentDto.StudentId &&
                                         e.CourseId == createEnrollmentDto.CourseId &&
                                         e.Status == EnrollmentStatus.Active);

            if (existingEnrollment != null)
            {
                throw new BadRequestException("Student is already enrolled in this course.");
            }

            var enrollment = new Enrollment
            {
                StudentId = createEnrollmentDto.StudentId,
                CourseId = createEnrollmentDto.CourseId,
                ClassId = createEnrollmentDto.ClassId,
                EnrollmentDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Enrollment created with ID: {EnrollmentId} for student {StudentId} in course {CourseId}",
                enrollment.Id, createEnrollmentDto.StudentId, createEnrollmentDto.CourseId);

            return await GetEnrollmentByIdAsync(enrollment.Id);
        }
        public async Task<EnrollmentDto> UpdateEnrollmentAsync(int id, UpdateEnrollmentDto updateEnrollmentDto)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new NotFoundException(nameof(Enrollment), id);
            }

            // Check if new class exists and belongs to the same course
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == updateEnrollmentDto.ClassId && c.CourseId == enrollment.CourseId);

            if (classEntity == null)
            {
                throw new NotFoundException("Class not found or does not belong to the enrolled course.");
            }

            enrollment.ClassId = updateEnrollmentDto.ClassId;

            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Enrollment updated with ID: {EnrollmentId}", id);

            return await GetEnrollmentByIdAsync(id);
        }
        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new NotFoundException(nameof(Enrollment), id);
            }

            _unitOfWork.Enrollments.Remove(enrollment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Enrollment deleted with ID: {EnrollmentId}", id);
            return true;
        }
        public async Task<bool> UpdateEnrollmentStatusAsync(int id, EnrollmentStatusDto statusDto)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
            if (enrollment == null)
            {
                throw new NotFoundException(nameof(Enrollment), id);
            }

            if (!Enum.TryParse<EnrollmentStatus>(statusDto.Status, out var status))
            {
                throw new BadRequestException("Invalid enrollment status.");
            }

            enrollment.Status = status;
            enrollment.CompletionDate = status == EnrollmentStatus.Completed ? DateTime.UtcNow : null;

            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Enrollment status updated to {Status} for enrollment ID: {EnrollmentId}", status, id);
            return true;
        }
        public async Task<APIResponseDto<EnrollmentDto>> GetAllEnrollmentsAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Class)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Student.User.FirstName.Contains(request.Search) ||
                    e.Student.User.LastName.Contains(request.Search) ||
                    e.Student.StudentId.Contains(request.Search) ||
                    e.Course.Name.Contains(request.Search) ||
                    e.Class.Name.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(e => e.Student.User.LastName) : query.OrderBy(e => e.Student.User.LastName),
                "course" => request.SortDescending ? query.OrderByDescending(e => e.Course.Name) : query.OrderBy(e => e.Course.Name),
                "class" => request.SortDescending ? query.OrderByDescending(e => e.Class.Name) : query.OrderBy(e => e.Class.Name),
                "date" => request.SortDescending ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
                "status" => request.SortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => query.OrderByDescending(e => e.EnrollmentDate)
            };

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var enrollmentDtos = _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);

            return new APIResponseDto<EnrollmentDto>(enrollmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }
        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Class)
                .Where(e => e.StudentId == studentId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Course.Name.Contains(request.Search) ||
                    e.Class.Name.Contains(request.Search) ||
                    e.Course.Code.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "course" => request.SortDescending ? query.OrderByDescending(e => e.Course.Name) : query.OrderBy(e => e.Course.Name),
                "class" => request.SortDescending ? query.OrderByDescending(e => e.Class.Name) : query.OrderBy(e => e.Class.Name),
                "date" => request.SortDescending ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
                "status" => request.SortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => query.OrderByDescending(e => e.EnrollmentDate)
            };

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var enrollmentDtos = _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);

            return new APIResponseDto<EnrollmentDto>(enrollmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }
        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId, SearchRequestDto request, string baseUrl)
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
        public async Task<APIResponseDto<EnrollmentDto>> GetEnrollmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Class)
                .Where(e => e.ClassId == classId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Student.User.FirstName.Contains(request.Search) ||
                    e.Student.User.LastName.Contains(request.Search) ||
                    e.Student.StudentId.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(e => e.Student.User.LastName) : query.OrderBy(e => e.Student.User.LastName),
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