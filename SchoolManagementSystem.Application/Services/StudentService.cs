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
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ILogger<StudentService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<User> userManager,
            IFileService fileService,
            ILogger<StudentService> logger,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.Query()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                throw new NotFoundException(nameof(Student), id);

            return _mapper.Map<StudentDto>(student);
        }

        public async Task<StudentDetailDto> GetStudentDetailAsync(int id)
        {
            var student = await _unitOfWork.Students.Query()
                .Include(s => s.User)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .Include(s => s.Grades)
                .Include(s => s.Attendances)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                throw new NotFoundException(nameof(Student), id);

            var dto = _mapper.Map<StudentDetailDto>(student);
            dto.TotalCourses = student.Enrollments.Count;
            dto.CompletedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
            dto.GPA = CalculateGPA(student.Grades);
            dto.AttendancePercentage = CalculateAttendancePercentage(student.Attendances);

            return dto;
        }

        public async Task<APIResponseDto<StudentDto>> GetPagedStudentsAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _unitOfWork.Students.Query()
                .Include(s => s.User)
                .Where(s => s.User.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(s =>
                    s.User.FirstName.Contains(request.Search) ||
                    s.User.LastName.Contains(request.Search) ||
                    s.StudentId.Contains(request.Search) ||
                    s.User.Email.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(s => s.User.LastName) : query.OrderBy(s => s.User.LastName),
                "studentid" => request.SortDescending ? query.OrderByDescending(s => s.StudentId) : query.OrderBy(s => s.StudentId),
                "email" => request.SortDescending ? query.OrderByDescending(s => s.User.Email) : query.OrderBy(s => s.User.Email),
                "enrollmentdate" => request.SortDescending ? query.OrderByDescending(s => s.EnrollmentDate) : query.OrderBy(s => s.EnrollmentDate),
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

        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
        {
            var existingUser = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new BadRequestException("A user with this email already exists.");

            if (!string.IsNullOrEmpty(dto.StudentId))
            {
                var existingStudent = await _unitOfWork.Students.Query().FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
                if (existingStudent != null)
                    throw new BadRequestException("A student with this ID already exists.");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var password = dto.Password ?? "Secret@123!";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Student);

            var student = new Student
            {
                UserId = user.Id,
                StudentId = dto.StudentId ?? GenerateStudentId(),
                DateOfBirth = dto.DateOfBirth,
                EnrollmentDate = DateTime.UtcNow,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student created: {StudentId}", student.Id);

            return await GetStudentByIdAsync(student.Id);
        }
        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            var student = await _unitOfWork.Students.Query()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                throw new NotFoundException(nameof(Student), id);

            student.User.FirstName = dto.FirstName;
            student.User.LastName = dto.LastName;
            student.User.PhoneNumber = dto.PhoneNumber;
            student.User.UpdatedDate = DateTime.UtcNow;

            student.DateOfBirth = dto.DateOfBirth;
            student.Address = dto.Address;
            student.PhoneNumber = dto.PhoneNumber;
            student.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student updated: {StudentId}", id);

            return _mapper.Map<StudentDto>(student);
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                throw new NotFoundException(nameof(Student), id);

            var user = await _unitOfWork.Users.GetByIdAsync(student.UserId);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
            }

            _logger.LogInformation("Student deleted: {StudentId}", id);
            return true;
        }

        public async Task<APIResponseDto<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId, SearchRequestDto request, string baseUrl)
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

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(e =>
                    e.Course.Name.Contains(request.Search) ||
                    e.Course.Code.Contains(request.Search) ||
                    e.Class.Name.Contains(request.Search));
            }

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

        public async Task<APIResponseDto<GradeDto>> GetStudentGradesAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Where(g => g.StudentId == studentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(g =>
                    g.Course.Name.Contains(request.Search) ||
                    g.AssessmentName.Contains(request.Search) ||
                    g.AssessmentType.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "course" => request.SortDescending ? query.OrderByDescending(g => g.Course.Name) : query.OrderBy(g => g.Course.Name),
                "date" => request.SortDescending ? query.OrderByDescending(g => g.AssessmentDate) : query.OrderBy(g => g.AssessmentDate),
                "score" => request.SortDescending ? query.OrderByDescending(g => g.Score) : query.OrderBy(g => g.Score),
                _ => query.OrderByDescending(g => g.AssessmentDate)
            };

            var totalCount = await query.CountAsync();

            var grades = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);

            return new APIResponseDto<GradeDto>(gradeDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<AttendanceDto>> GetStudentAttendanceAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .Where(a => a.StudentId == studentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Class.Name.Contains(request.Search) ||
                    a.Remarks.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "class" => request.SortDescending ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
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

        public async Task<StudentStatsDto> GetStudentStatsAsync(int studentId)
        {
            var student = await _unitOfWork.Students.Query()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Include(s => s.Grades)
                .Include(s => s.Attendances)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            return new StudentStatsDto
            {
                StudentId = student.Id,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                TotalCourses = student.Enrollments.Count,
                CompletedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
                ActiveCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                OverallGPA = CalculateGPA(student.Grades),
                AttendancePercentage = CalculateAttendancePercentage(student.Attendances),
                TotalAssignments = student.Grades.Count,
                CompletedAssignments = student.Grades.Count(g => g.Score > 0),
                LastLogin = student.User.UpdatedDate
            };
        }

        public async Task<bool> UploadStudentPhotoAsync(StudentPhotoDto dto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(dto.StudentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), dto.StudentId);

            var folderPath = $"uploads/students/{student.User.FullName}/photos";
            var photoUrl = await _fileService.SaveFileAsync(dto.File, folderPath);

            _logger.LogInformation("Photo uploaded for student {StudentId}: {PhotoUrl}", dto.StudentId, photoUrl);

            return true;
        }

        public async Task<APIResponseDto<AssignmentDto>> GetStudentAssignmentsAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var classIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.ClassId)
                .ToListAsync();

            var query = _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(a => classIds.Contains(a.ClassId) && a.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.Search) ||
                    a.Description.Contains(request.Search) ||
                    a.Class.Course.Name.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "course" => request.SortDescending ? query.OrderByDescending(a => a.Class.Course.Name) : query.OrderBy(a => a.Class.Course.Name),
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

        //public async Task<bool> SubmitAssignmentAsync(SubmitAssignmentDto dto)
        //{
        //    var assignment = await _unitOfWork.Assignments.Query()
        //        .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId /*&& a. == dto.StudentId*/);

        //    if (assignment == null)
        //        throw new NotFoundException($"Assignment {dto.AssignmentId} not found for student {dto.StudentId}");

        //    // var folderPath = $"uploads/students/{assignment.Student.User.FullName}/assignments";
        //    var folderPath = $"uploads/students/{assignment}/assignments";
        //    var fileUrl = await _fileService.SaveFileAsync(dto.File, folderPath);

        //    //assignment.FileUrl = fileUrl;
        //    //assignment.SubmittedAt = DateTime.UtcNow;
        //    //assignment.Status = AssignmentStatus.Submitted;

        //    _unitOfWork.Assignments.Update(assignment);
        //    await _unitOfWork.CompleteAsync();

        //    _logger.LogInformation("Assignment {AssignmentId} submitted by student {StudentId}", dto.AssignmentId, dto.StudentId);
        //    return true;
        //}

        public async Task<APIResponseDto<NotificationDto>> GetStudentNotificationsAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Notifications
                .Where(n => n.RecipientRole == "All" ||
                           n.RecipientRole == "Student" ||
                           n.RecipientId == studentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(n =>
                    n.Title.Contains(request.Search) ||
                    n.Message.Contains(request.Search));
            }

            query = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
                "date" => request.SortDescending ? query.OrderByDescending(n => n.CreatedDate) : query.OrderBy(n => n.CreatedDate),
                "read" => request.SortDescending ? query.OrderByDescending(n => n.IsRead) : query.OrderBy(n => n.IsRead),
                _ => query.OrderByDescending(n => n.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

            return new APIResponseDto<NotificationDto>(notificationDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }
        public async Task<APIResponseDto<ClassDto>> GetStudentClassesAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Enrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Include(e => e.Class)
                    .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.Class)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(c =>
                    c.Name.Contains(request.Search) ||
                    c.Course.Name.Contains(request.Search) ||
                    (c.Teacher != null && c.Teacher.User.FirstName.Contains(request.Search)) ||
                    (c.Teacher != null && c.Teacher.User.LastName.Contains(request.Search)));
            }

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "course" => request.SortDescending ? query.OrderByDescending(c => c.Course.Name) : query.OrderBy(c => c.Course.Name),
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

        private static decimal CalculateGPA(ICollection<Grade> grades)
        {
            if (!grades.Any()) return 0;

            var courseGrades = grades
                .GroupBy(g => g.CourseId)
                .Select(g => new
                {
                    Average = g.Average(x => x.Percentage),
                    Credits = 3
                }).ToList();

            var totalPoints = courseGrades.Sum(cg => GradeCalculator.GetGradePoints(GradeCalculator.CalculateGrade(cg.Average)) * cg.Credits);
            var totalCredits = courseGrades.Sum(cg => cg.Credits);

            return totalCredits > 0 ? totalPoints / totalCredits : 0;
        }

        private static decimal CalculateAttendancePercentage(ICollection<Attendance> attendances)
        {
            if (!attendances.Any()) return 0;
            var total = attendances.Count;
            var present = attendances.Count(a => a.Status == AttendanceStatus.Present);
            return total > 0 ? (decimal)present / total * 100 : 0;
        }
        public async Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .Include(s => s.Grades)
                .Include(s => s.Attendances)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var dashboard = new StudentDashboardDto
            {
                StudentId = student.Id,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentIdNumber = student.StudentId,
                EnrollmentDate = student.EnrollmentDate,
                LastLogin = student.User.UpdatedDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Current enrollments
            var currentEnrollments = student.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Active)
                .ToList();

            dashboard.CurrentCourses = currentEnrollments.Count;
            dashboard.CompletedCourses = student.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            // Recent grades (last 10)
            var recentGrades = student.Grades
                .OrderByDescending(g => g.AssessmentDate)
                .Take(10)
                .ToList();

            dashboard.RecentGrades = _mapper.Map<List<GradeDto>>(recentGrades);

            // Upcoming assignments (due in next 7 days)
            var classIds = currentEnrollments.Select(e => e.ClassId).ToList();
            var upcomingAssignments = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Where(a => classIds.Contains(a.ClassId) &&
                           a.DueDate >= DateTime.UtcNow &&
                           a.DueDate <= DateTime.UtcNow.AddDays(7) &&
                           a.IsActive == true)
                .OrderBy(a => a.DueDate)
                .Take(5)
                .ToListAsync();

            dashboard.UpcomingAssignments = _mapper.Map<List<AssignmentDto>>(upcomingAssignments);

            // Attendance summary for current courses
            var recentAttendances = student.Attendances
                .Where(a => a.Date >= DateTime.UtcNow.AddDays(-30)) // Last 30 days
                .ToList();

            dashboard.RecentAttendance = new AttendanceSummaryDto
            {
                TotalClasses = recentAttendances.Count,
                Present = recentAttendances.Count(a => a.Status == AttendanceStatus.Present),
                Absent = recentAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                Late = recentAttendances.Count(a => a.Status == AttendanceStatus.Late),
                Excused = recentAttendances.Count(a => a.Status == AttendanceStatus.Excused)
            };

            dashboard.RecentAttendance.AttendancePercentage = dashboard.RecentAttendance.TotalClasses > 0 ?
                (decimal)(dashboard.RecentAttendance.Present + dashboard.RecentAttendance.Excused) /
                dashboard.RecentAttendance.TotalClasses * 100 : 0;

            // Course performance
            foreach (var enrollment in currentEnrollments)
            {
                var courseGrades = student.Grades
                    .Where(g => g.CourseId == enrollment.CourseId)
                    .ToList();

                var averageGrade = courseGrades.Any() ? courseGrades.Average(g => g.Percentage) : 0;
                var gradeLetter = GradeCalculator.CalculateGrade(averageGrade).ToString();

                dashboard.CoursePerformance.Add(new CoursePerformanceDto
                {
                    CourseId = enrollment.CourseId,
                    CourseCode = enrollment.Course.Code,
                    CourseName = enrollment.Course.Name,
                    ClassName = enrollment.Class.Name,
                    AverageGrade = averageGrade,
                    GradeLetter = gradeLetter,
                    LastUpdated = courseGrades.Any() ? courseGrades.Max(g => g.AssessmentDate) : null
                });
            }

            // Overall GPA
            dashboard.OverallGPA = CalculateGPA(student.Grades);

            // Pending submissions
            var submittedAssignmentIds = await _context.Submissions
                .Where(s => s.StudentId == studentId)
                .Select(s => s.AssignmentId)
                .ToListAsync();

            var pendingAssignments = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Where(a => classIds.Contains(a.ClassId) &&
                           !submittedAssignmentIds.Contains(a.Id) &&
                           a.DueDate >= DateTime.UtcNow &&
                           a.IsActive == true)
                .OrderBy(a => a.DueDate)
                .Take(5)
                .ToListAsync();

            dashboard.PendingSubmissions = _mapper.Map<List<AssignmentDto>>(pendingAssignments);

            // Recent notifications
            var recentNotifications = await _context.Notifications
                .Where(n => n.RecipientRole == "All" ||
                           n.RecipientRole == "Student" ||
                           n.RecipientId == studentId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToListAsync();

            dashboard.RecentNotifications = _mapper.Map<List<NotificationDto>>(recentNotifications);

            _logger.LogInformation("Dashboard generated for student {StudentId}", studentId);
            return dashboard;
        }
        private static string GenerateStudentId()
        {
            return $"STU{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}
