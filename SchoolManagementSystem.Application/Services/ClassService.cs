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
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassService> _logger;

        public ClassService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<ClassService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<ClassDto> GetClassByIdAsync(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), id);
            }

            var classDto = _mapper.Map<ClassDto>(classEntity);
            classDto.TotalStudents = await _context.Enrollments
                .CountAsync(e => e.ClassId == id && e.Status == Core.Enums.EnrollmentStatus.Active);

            return classDto;
        }

        public async Task<ClassDetailDto> GetClassDetailAsync(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), id);
            }

            return _mapper.Map<ClassDetailDto>(classEntity);
        }

        public async Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            var existingClass = await _context.Classes
                .FirstOrDefaultAsync(c => c.CourseId == createClassDto.CourseId &&
                                         c.Name == createClassDto.Name &&
                                         c.Section == createClassDto.Section);

            if (existingClass != null)
            {
                throw new BadRequestException("A class with this name and section already exists for this course.");
            }

            var classEntity = _mapper.Map<Class>(createClassDto);
            classEntity.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Classes.AddAsync(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class created with ID: {ClassId}", classEntity.Id);

            return await GetClassByIdAsync(classEntity.Id);
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), id);
            }
            // Check if class with same name and section already exists for the course (excluding current class)
            var existingClass = await _context.Classes
                .FirstOrDefaultAsync(c => c.CourseId == classEntity.CourseId &&
                                         c.Name == updateClassDto.Name &&
                                         c.Section == updateClassDto.Section &&
                                         c.Id != id);

            if (existingClass != null)
            {
                throw new BadRequestException("A class with this name and section already exists for this course.");
            }

            _mapper.Map(updateClassDto, classEntity);
            classEntity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Classes.Update(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class updated with ID: {ClassId}", id);

            return await GetClassByIdAsync(id);
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), id);
            }

            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.ClassId == id);
            if (hasEnrollments)
            {
                throw new BadRequestException("Cannot delete class with existing enrollments.");
            }

            _unitOfWork.Classes.Remove(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class deleted with ID: {ClassId}", id);
            return true;
        }

       
        public async Task<bool> AssignTeacherToClassAsync(int classId, int teacherId)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(classId);
            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), classId);
            }

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), teacherId);
            }

            classEntity.TeacherId = teacherId;
            classEntity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Classes.Update(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Teacher {TeacherId} assigned to class {ClassId}", teacherId, classId);
            return true;
        }

        public async Task<bool> RemoveTeacherFromClassAsync(int classId)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(classId);
            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), classId);
            }

            classEntity.TeacherId = 0;
            classEntity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Classes.Update(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Teacher removed from class {ClassId}", classId);
            return true;
        }
        public async Task<APIResponseDto<ClassDto>> GetAllClassesAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.Search) ||
                    c.Section.Contains(request.Search) ||
                    c.Course.Name.Contains(request.Search) ||
                    (c.Teacher != null && c.Teacher.User.FirstName.Contains(request.Search)) ||
                    (c.Teacher != null && c.Teacher.User.LastName.Contains(request.Search)));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "course" => request.SortDescending ? query.OrderByDescending(c => c.Course.Name) : query.OrderBy(c => c.Course.Name),
                "teacher" => request.SortDescending ? query.OrderByDescending(c => c.Teacher.User.LastName) : query.OrderBy(c => c.Teacher.User.LastName),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();

            var classes = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var classDtos = _mapper.Map<IEnumerable<ClassDto>>(classes);

            // Add student counts
            foreach (var classDto in classDtos)
            {
                classDto.TotalStudents = await _context.Enrollments
                    .CountAsync(e => e.ClassId == classDto.Id && e.Status == EnrollmentStatus.Active);
            }

            return new APIResponseDto<ClassDto>(classDtos, request.Page, request.PageSize, totalCount, baseUrl, "Data Found");
        }
        public async Task<APIResponseDto<EnrollmentDto>> GetClassEnrollmentsAsync(int classId, SearchRequestDto request, string baseUrl)
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

            return new APIResponseDto<EnrollmentDto>(enrollmentDtos, request.Page, request.PageSize, totalCount, baseUrl,"Data Found");
        }
    }
}