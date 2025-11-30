using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<TeacherService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<TeacherDto> GetTeacherByIdAsync(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), id);
            }

            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task<TeacherDetailDto> GetTeacherDetailAsync(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Courses)
                .Include(t => t.Classes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), id);
            }

            var teacherDetail = _mapper.Map<TeacherDetailDto>(teacher);

            teacherDetail.TotalCourses = teacher.Courses.Count;
            teacherDetail.TotalClasses = teacher.Classes.Count;
            teacherDetail.TotalStudents = await _context.Enrollments
                .Where(e => e.Class.TeacherId == id || e.Course.TeacherId == id)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            return teacherDetail;
        }

        public async Task<TeacherDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createTeacherDto.Email);
            if (existingUser != null)
            {
                throw new BadRequestException("A user with this email already exists.");
            }

            if (!string.IsNullOrEmpty(createTeacherDto.EmployeeId))
            {
                var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.EmployeeId == createTeacherDto.EmployeeId);
                if (existingTeacher != null)
                {
                    throw new BadRequestException("A teacher with this employee ID already exists.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    FirstName = createTeacherDto.FirstName,
                    LastName = createTeacherDto.LastName,
                    UserName = createTeacherDto.Email,
                    Email = createTeacherDto.Email,
                    PhoneNumber = createTeacherDto.PhoneNumber,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var password = createTeacherDto.Password ?? "DefaultPassword123!";
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new BadRequestException($"User creation failed: {errors}");
                }

                await _userManager.AddToRoleAsync(user, UserRoles.Teacher);

                var teacher = new Teacher
                {
                    UserId = user.Id,
                    EmployeeId = createTeacherDto.EmployeeId ?? GenerateEmployeeId(),
                    Department = createTeacherDto.Department,
                    Qualification = createTeacherDto.Qualification,
                    HireDate = DateTime.UtcNow,
                    Salary = createTeacherDto.Salary
                };

                await _unitOfWork.Teachers.AddAsync(teacher);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Teacher created with ID: {TeacherId}", teacher.Id);

                return await GetTeacherByIdAsync(teacher.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TeacherDto> UpdateTeacherAsync(int id, UpdateTeacherDto updateTeacherDto)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), id);
            }

            teacher.User.FirstName = updateTeacherDto.FirstName;
            teacher.User.LastName = updateTeacherDto.LastName;
            teacher.User.PhoneNumber = updateTeacherDto.PhoneNumber;
            teacher.User.UpdatedDate = DateTime.UtcNow;

            teacher.Department = updateTeacherDto.Department;
            teacher.Qualification = updateTeacherDto.Qualification;
            teacher.Salary = updateTeacherDto.Salary;
            teacher.UpdatedDate = DateTime.UtcNow;

            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher updated with ID: {TeacherId}", id);

            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(id);
            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), id);
            }

            var user = await _context.Users.FindAsync(teacher.UserId);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Teacher deleted with ID: {TeacherId}", id);
            return true;
        }

        public async Task<TeacherStatsDto> GetTeacherStatsAsync(int teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Courses)
                .Include(t => t.Classes)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                throw new NotFoundException(nameof(Teacher), teacherId);
            }

            var stats = new TeacherStatsDto
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                TotalCourses = teacher.Courses.Count,
                TotalClasses = teacher.Classes.Count,
                TotalStudents = await _context.Enrollments
                    .Where(e => e.Class.TeacherId == teacherId || e.Course.TeacherId == teacherId)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .CountAsync(),
                TotalAssignments = await _context.Assignments
                    .Where(a => a.CreatedByTeacherId == teacherId)
                    .CountAsync(),
                AverageStudentRating = await CalculateAverageRating(teacherId),
                LastLogin = teacher.User.UpdatedDate
            };

            return stats;
        }

        public async Task<APIResponseDto<TeacherDto>> GetAllTeachersAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Teachers
                .Include(t => t.User)
                .Where(t => t.User.IsActive)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(t =>
                    t.User.FirstName.Contains(request.Search) ||
                    t.User.LastName.Contains(request.Search) ||
                    t.EmployeeId.Contains(request.Search) ||
                    t.Department.Contains(request.Search) ||
                    t.Qualification.Contains(request.Search) ||
                    t.User.Email.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(t => t.User.LastName) : query.OrderBy(t => t.User.LastName),
                "employeeid" => request.SortDescending ? query.OrderByDescending(t => t.EmployeeId) : query.OrderBy(t => t.EmployeeId),
                "department" => request.SortDescending ? query.OrderByDescending(t => t.Department) : query.OrderBy(t => t.Department),
                "email" => request.SortDescending ? query.OrderByDescending(t => t.User.Email) : query.OrderBy(t => t.User.Email),
                _ => query.OrderBy(t => t.User.LastName).ThenBy(t => t.User.FirstName)
            };

            var totalCount = await query.CountAsync();

            var teachers = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var teacherDtos = _mapper.Map<IEnumerable<TeacherDto>>(teachers);

            return new APIResponseDto<TeacherDto>(teacherDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<CourseDto>> GetTeacherCoursesAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException(nameof(Teacher), teacherId);

            var query = _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c => c.TeacherId == teacherId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.Search) ||
                    c.Code.Contains(request.Search) ||
                    c.Description.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
                "credits" => request.SortDescending ? query.OrderByDescending(c => c.Credits) : query.OrderBy(c => c.Credits),
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

        public async Task<APIResponseDto<ClassDto>> GetTeacherClassesAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException(nameof(Teacher), teacherId);

            var query = _context.Classes
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Course)
                .Where(c => c.TeacherId == teacherId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.Search) ||
                    c.Section.Contains(request.Search) ||
                    c.Course.Name.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "course" => request.SortDescending ? query.OrderByDescending(c => c.Course.Name) : query.OrderBy(c => c.Course.Name),
                "section" => request.SortDescending ? query.OrderByDescending(c => c.Section) : query.OrderBy(c => c.Section),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();

            var classes = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var classDtos = _mapper.Map<IEnumerable<ClassDto>>(classes);

            return new APIResponseDto<ClassDto>(classDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<bool> EnrollStudentInClassAsync(int classId, int studentId, int teacherId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            // Teacher authorization check
            if (classEntity.TeacherId != teacherId)
                throw new UnauthorizedException("You are not assigned to teach this class.");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            // Check if student is already enrolled in this class
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                         e.ClassId == classId &&
                                         e.Status == EnrollmentStatus.Active);

            if (existingEnrollment != null)
                throw new BadRequestException("Student is already enrolled in this class.");

            // Check if student is enrolled in the course but different class
            var courseEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                         e.CourseId == classEntity.CourseId &&
                                         e.Status == EnrollmentStatus.Active);

            if (courseEnrollment != null)
            {
                // Update existing enrollment to new class
                courseEnrollment.ClassId = classId;
                courseEnrollment.UpdatedDate = DateTime.UtcNow;
                _context.Enrollments.Update(courseEnrollment);
            }
            else
            {
                // Create new enrollment
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = classEntity.CourseId ?? 0,
                    ClassId = classId,
                    EnrollmentDate = DateTime.UtcNow,
                    Status = EnrollmentStatus.Active,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Enrollments.AddAsync(enrollment);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student {StudentId} enrolled in class {ClassId} by teacher {TeacherId}", studentId, classId, teacherId);
            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int classId, int studentId)
        {
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                         e.ClassId == classId &&
                                         e.Status == EnrollmentStatus.Active);

            if (enrollment == null)
                throw new BadRequestException("Student is not enrolled in this class.");

            // Soft delete by setting status to Withdrawn
            enrollment.Status = EnrollmentStatus.Withdrawn;
            enrollment.UpdatedDate = DateTime.UtcNow;

            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student {StudentId} removed from class {ClassId}", studentId, classId);
            return true;
        }

        public async Task<APIResponseDto<StudentDto>> GetClassStudentsAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            var query = _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.Student)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(s =>
                    s.User.FirstName.Contains(request.Search) ||
                    s.User.LastName.Contains(request.Search) ||
                    s.StudentId.Contains(request.Search) ||
                    s.User.Email.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(s => s.User.LastName) : query.OrderBy(s => s.User.LastName),
                "studentid" => request.SortDescending ? query.OrderByDescending(s => s.StudentId) : query.OrderBy(s => s.StudentId),
                "email" => request.SortDescending ? query.OrderByDescending(s => s.User.Email) : query.OrderBy(s => s.User.Email),
                _ => query.OrderBy(s => s.User.LastName).ThenBy(s => s.User.FirstName)
            };

            var totalCount = await query.CountAsync();

            var students = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(students);

            return new APIResponseDto<StudentDto>(studentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<BulkEnrollmentResultDto> BulkEnrollStudentsAsync(int classId, List<int> studentIds, int teacherId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            // Teacher authorization check
            if (classEntity.TeacherId != teacherId)
                throw new UnauthorizedException("You are not assigned to teach this class.");

            if (classEntity.CourseId == null)
                throw new BadRequestException("Class must be associated with a course for enrollment.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = new BulkEnrollmentResultDto();
                var successfulEnrollments = 0;

                foreach (var studentId in studentIds.Distinct())
                {
                    try
                    {
                        var student = await _context.Students
                            .Include(s => s.User)
                            .FirstOrDefaultAsync(s => s.Id == studentId);

                        if (student == null)
                        {
                            result.FailureDetails.Add($"Student ID {studentId} not found");
                            continue;
                        }

                        // Check if already enrolled
                        var existingEnrollment = await _context.Enrollments
                            .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                                     e.ClassId == classId &&
                                                     e.Status == EnrollmentStatus.Active);

                        if (existingEnrollment != null)
                        {
                            result.FailureDetails.Add($"Student {student.User.FullName} already enrolled");
                            continue;
                        }

                        // Check for existing course enrollment
                        var courseEnrollment = await _context.Enrollments
                            .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                                     e.CourseId == classEntity.CourseId &&
                                                     e.Status == EnrollmentStatus.Active);

                        if (courseEnrollment != null)
                        {
                            // Update to new class
                            courseEnrollment.ClassId = classId;
                            courseEnrollment.UpdatedDate = DateTime.UtcNow;
                            _context.Enrollments.Update(courseEnrollment);
                        }
                        else
                        {
                            // Create new enrollment
                            var enrollment = new Enrollment
                            {
                                StudentId = studentId,
                                CourseId = classEntity.CourseId.Value,
                                ClassId = classId,
                                EnrollmentDate = DateTime.UtcNow,
                                Status = EnrollmentStatus.Active,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _unitOfWork.Enrollments.AddAsync(enrollment);
                        }

                        successfulEnrollments++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureDetails.Add($"Student ID {studentId}: {ex.Message}");
                    }
                }

                result.TotalProcessed = studentIds.Count;
                result.Successful = successfulEnrollments;
                result.Failed = result.FailureDetails.Count;

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                if (result.FailureDetails.Any())
                {
                    _logger.LogWarning("Bulk enrollment completed with {SuccessCount} successes and {FailCount} failures",
                        successfulEnrollments, result.FailureDetails.Count);
                }
                else
                {
                    _logger.LogInformation("Bulk enrollment completed successfully for {Count} students in class {ClassId}",
                        successfulEnrollments, classId);
                }

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<ClassDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            var course = await _context.Courses.FindAsync(createClassDto.CourseId);
            if (course == null)
                throw new NotFoundException(nameof(Course), createClassDto.CourseId);

            var teacher = await _context.Teachers.FindAsync(createClassDto.TeacherId);
            if (teacher == null)
                throw new NotFoundException(nameof(Teacher), createClassDto.TeacherId);

            // Check for duplicate class
            var existingClass = await _context.Classes
                .FirstOrDefaultAsync(c => c.Name == createClassDto.Name &&
                                         c.Section == createClassDto.Section &&
                                         c.CourseId == createClassDto.CourseId);

            if (existingClass != null)
                throw new BadRequestException("A class with this name and section already exists for this course.");

            var classEntity = _mapper.Map<Class>(createClassDto);
            classEntity.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Classes.AddAsync(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class created with ID: {ClassId} by teacher {TeacherId}",
                classEntity.Id, createClassDto.TeacherId);

            return await GetClassByIdAsync(classEntity.Id);
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto updateClassDto)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), id);

            // Check for duplicate class name
            var existingClass = await _context.Classes
                .FirstOrDefaultAsync(c => c.Name == updateClassDto.Name &&
                                         c.Section == updateClassDto.Section &&
                                         c.CourseId == classEntity.CourseId &&
                                         c.Id != id);

            if (existingClass != null)
                throw new BadRequestException("A class with this name and section already exists for this course.");

            _mapper.Map(updateClassDto, classEntity);
            classEntity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Classes.Update(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class updated with ID: {ClassId}", id);

            return await GetClassByIdAsync(id);
        }

        public async Task<bool> DeactivateClassAsync(int id)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), id);

            // Check if class has active enrollments
            var hasActiveEnrollments = await _context.Enrollments
                .AnyAsync(e => e.ClassId == id && e.Status == EnrollmentStatus.Active);

            if (hasActiveEnrollments)
                throw new BadRequestException("Cannot deactivate class with active enrollments.");

            // In a real scenario, you might want to soft delete or set IsActive = false
            // For now, we'll remove the class
            _unitOfWork.Classes.Remove(classEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Class deactivated with ID: {ClassId}", id);
            return true;
        }
        public async Task<ClassDto> GetClassByIdAsync(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), id);

            var classDto = _mapper.Map<ClassDto>(classEntity);
            classDto.TotalStudents = await _context.Enrollments
                .CountAsync(e => e.ClassId == id && e.Status == EnrollmentStatus.Active);

            return classDto;
        }
        public async Task<APIResponseDto<AttendanceDto>> GetClassAttendanceHistoryAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .Where(a => a.ClassId == classId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Student.User.FirstName.Contains(request.Search) ||
                    a.Student.User.LastName.Contains(request.Search) ||
                    a.Remarks.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(a => a.Student.User.LastName) : query.OrderBy(a => a.Student.User.LastName),
                "date" => request.SortDescending ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date),
                "status" => request.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                _ => query.OrderByDescending(a => a.Date)
            };

            var totalCount = await query.CountAsync();

            var attendances = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);

            return new APIResponseDto<AttendanceDto>(attendanceDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<AssignmentDto>> GetClassAssignmentsAsync(int classId, SearchRequestDto request, string baseUrl)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), classId);

            var query = _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.ClassId == classId && a.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.Search) ||
                    a.Description.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "duedate" => request.SortDescending ? query.OrderByDescending(a => a.DueDate) : query.OrderBy(a => a.DueDate),
                _ => query.OrderByDescending(a => a.DueDate)
            };

            var totalCount = await query.CountAsync();

            var assignments = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var assignmentDtos = _mapper.Map<IEnumerable<AssignmentDto>>(assignments);

            return new APIResponseDto<AssignmentDto>(assignmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }
        private static Task<decimal> CalculateAverageRating(int teacherId)
        {
            // Placeholder implementation - in real app, this would calculate from student ratings
            return Task.FromResult(4.5m);
        }

        private static string GenerateEmployeeId()
        {
            return $"TCH{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}