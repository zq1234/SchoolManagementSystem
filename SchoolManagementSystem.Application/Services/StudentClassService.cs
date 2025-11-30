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

namespace SchoolManagementSystem.Infrastructure.Services
{
    //public class StudentClassService : IStudentClassService
    //{
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IMapper _mapper;
    //    private readonly ApplicationDbContext _context;
    //    private readonly ILogger<StudentClassService> _logger;

    //    public StudentClassService(
    //        IUnitOfWork unitOfWork,
    //        IMapper mapper,
    //        ApplicationDbContext context,
    //        ILogger<StudentClassService> logger)
    //    {
    //        _unitOfWork = unitOfWork;
    //        _mapper = mapper;
    //        _context = context;
    //        _logger = logger;
    //    }

    //    public async Task<StudentClassDto> GetStudentClassByIdAsync(int id)
    //    {
    //        var studentClass = await _context.StudentClasses
    //            .Include(sc => sc.Student)
    //                .ThenInclude(s => s.User)
    //            .Include(sc => sc.Class)
    //                .ThenInclude(c => c.Course)
    //            .FirstOrDefaultAsync(sc => sc.Id == id);

    //        if (studentClass == null)
    //            throw new NotFoundException(nameof(StudentClass), id);

    //        return _mapper.Map<StudentClassDto>(studentClass);
    //    }

    //    public async Task<StudentClassDto> EnrollStudentAsync(CreateStudentClassDto createStudentClassDto)
    //    {
    //        var student = await _context.Students
    //            .Include(s => s.User)
    //            .FirstOrDefaultAsync(s => s.Id == createStudentClassDto.StudentId);

    //        if (student == null)
    //            throw new NotFoundException(nameof(Student), createStudentClassDto.StudentId);

    //        var classEntity = await _context.Classes
    //            .Include(c => c.Course)
    //            .FirstOrDefaultAsync(c => c.Id == createStudentClassDto.ClassId);

    //        if (classEntity == null)
    //            throw new NotFoundException(nameof(Class), createStudentClassDto.ClassId);

    //        // Check if already enrolled
    //        var existingEnrollment = await _context.StudentClasses
    //            .FirstOrDefaultAsync(sc => sc.StudentId == createStudentClassDto.StudentId &&
    //                                      sc.ClassId == createStudentClassDto.ClassId);

    //        if (existingEnrollment != null)
    //            throw new BadRequestException("Student is already enrolled in this class.");

    //        var studentClass = new StudentClass
    //        {
    //            StudentId = createStudentClassDto.StudentId,
    //            ClassId = createStudentClassDto.ClassId,
    //            EnrollmentDate = DateTime.UtcNow,
    //            CreatedDate = DateTime.UtcNow
    //        };

    //        await _unitOfWork.StudentClasses.AddAsync(studentClass);
    //        await _unitOfWork.CompleteAsync();

    //        _logger.LogInformation("Student {StudentId} enrolled in class {ClassId}", createStudentClassDto.StudentId, createStudentClassDto.ClassId);

    //        return await GetStudentClassByIdAsync(studentClass.Id);
    //    }

    //    public async Task<bool> RemoveStudentFromClassAsync(int studentId, int classId)
    //    {
    //        var studentClass = await _context.StudentClasses
    //            .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.ClassId == classId);

    //        if (studentClass == null)
    //            throw new NotFoundException("Student enrollment not found.");

    //        _unitOfWork.StudentClasses.Remove(studentClass);
    //        await _unitOfWork.CompleteAsync();

    //        _logger.LogInformation("Student {StudentId} removed from class {ClassId}", studentId, classId);
    //        return true;
    //    }

    //    public async Task<APIResponseDto<StudentClassDto>> GetClassEnrollmentsAsync(int classId, SearchRequestDto request, string baseUrl)
    //    {
    //        var classEntity = await _context.Classes.FindAsync(classId);
    //        if (classEntity == null)
    //            throw new NotFoundException(nameof(Class), classId);

    //        var query = _context.StudentClasses
    //            .Include(sc => sc.Student)
    //                .ThenInclude(s => s.User)
    //            .Include(sc => sc.Class)
    //                .ThenInclude(c => c.Course)
    //            .Where(sc => sc.ClassId == classId)
    //            .AsQueryable();

    //        // Apply search filter
    //        if (!string.IsNullOrWhiteSpace(request.Search))
    //        {
    //            query = query.Where(sc =>
    //                sc.Student.User.FirstName.Contains(request.Search) ||
    //                sc.Student.User.LastName.Contains(request.Search) ||
    //                sc.Student.StudentId.Contains(request.Search));
    //        }

    //        // Apply sorting
    //        query = request.SortBy?.ToLower() switch
    //        {
    //            "student" => request.SortDescending ? query.OrderByDescending(sc => sc.Student.User.LastName) : query.OrderBy(sc => sc.Student.User.LastName),
    //            "date" => request.SortDescending ? query.OrderByDescending(sc => sc.EnrollmentDate) : query.OrderBy(sc => sc.EnrollmentDate),
    //            _ => query.OrderBy(sc => sc.Student.User.LastName)
    //        };

    //        var totalCount = await query.CountAsync();

    //        var studentClasses = await query
    //            .Skip(request.Skip)
    //            .Take(request.Take)
    //            .ToListAsync();

    //        var studentClassDtos = _mapper.Map<IEnumerable<StudentClassDto>>(studentClasses);

    //        return new APIResponseDto<StudentClassDto>(studentClassDtos, request.Page, request.PageSize, totalCount, baseUrl);
    //    }

    //    public async Task<APIResponseDto<StudentClassDto>> GetStudentEnrollmentsAsync(int studentId, SearchRequestDto request, string baseUrl)
    //    {
    //        var student = await _context.Students.FindAsync(studentId);
    //        if (student == null)
    //            throw new NotFoundException(nameof(Student), studentId);

    //        var query = _context.StudentClasses
    //            .Include(sc => sc.Student)
    //                .ThenInclude(s => s.User)
    //            .Include(sc => sc.Class)
    //                .ThenInclude(c => c.Course)
    //            .Where(sc => sc.StudentId == studentId)
    //            .AsQueryable();

    //        // Apply search filter
    //        if (!string.IsNullOrWhiteSpace(request.Search))
    //        {
    //            query = query.Where(sc =>
    //                sc.Class.Name.Contains(request.Search) ||
    //                sc.Class.Course.Name.Contains(request.Search));
    //        }

    //        // Apply sorting
    //        query = request.SortBy?.ToLower() switch
    //        {
    //            "class" => request.SortDescending ? query.OrderByDescending(sc => sc.Class.Name) : query.OrderBy(sc => sc.Class.Name),
    //            "course" => request.SortDescending ? query.OrderByDescending(sc => sc.Class.Course.Name) : query.OrderBy(sc => sc.Class.Course.Name),
    //            "date" => request.SortDescending ? query.OrderByDescending(sc => sc.EnrollmentDate) : query.OrderBy(sc => sc.EnrollmentDate),
    //            _ => query.OrderByDescending(sc => sc.EnrollmentDate)
    //        };

    //        var totalCount = await query.CountAsync();

    //        var studentClasses = await query
    //            .Skip(request.Skip)
    //            .Take(request.Take)
    //            .ToListAsync();

    //        var studentClassDtos = _mapper.Map<IEnumerable<StudentClassDto>>(studentClasses);

    //        return new APIResponseDto<StudentClassDto>(studentClassDtos, request.Page, request.PageSize, totalCount, baseUrl);
    //    }

    //    public async Task<int> GetClassStudentCountAsync(int classId)
    //    {
    //        return await _context.StudentClasses
    //            .CountAsync(sc => sc.ClassId == classId);
    //    }

    //    public async Task<bool> IsStudentEnrolledAsync(int studentId, int classId)
    //    {
    //        return await _context.StudentClasses
    //            .AnyAsync(sc => sc.StudentId == studentId && sc.ClassId == classId);
    //    }
    //}
}