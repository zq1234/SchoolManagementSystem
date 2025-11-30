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
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<AttendanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<AttendanceDto> GetAttendanceByIdAsync(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                throw new NotFoundException(nameof(Attendance), id);
            }

            return _mapper.Map<AttendanceDto>(attendance);
        }
        
        public async Task<APIResponseDto<AttendanceDto>> GetAllAttendanceAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Student.User.FirstName.Contains(request.Search) ||
                    a.Student.User.LastName.Contains(request.Search) ||
                    a.Class.Name.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "date" => request.SortDescending ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date),
                "student" => request.SortDescending ? query.OrderByDescending(a => a.Student.User.LastName) : query.OrderBy(a => a.Student.User.LastName),
                "class" => request.SortDescending ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
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

        public async Task<AttendanceDto> CreateAttendanceAsync(CreateAttendanceDto createAttendanceDto)
        {
            // Check if student exists
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == createAttendanceDto.StudentId);

            if (student == null)
                throw new NotFoundException(nameof(Student), createAttendanceDto.StudentId);

            // Check if class exists
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == createAttendanceDto.ClassId);
            if (classEntity == null)
                throw new NotFoundException(nameof(Class), createAttendanceDto.ClassId);

            // Check if student is enrolled in the class
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == createAttendanceDto.StudentId &&
                                         e.ClassId == createAttendanceDto.ClassId &&
                                         e.Status == EnrollmentStatus.Active);

            if (enrollment == null)
                throw new BadRequestException("Student is not enrolled in this class.");

            // Check if attendance already exists for this student, class, and date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == createAttendanceDto.StudentId &&
                                         a.ClassId == createAttendanceDto.ClassId &&
                                         a.Date.Date == createAttendanceDto.Date.Date);

            if (existingAttendance != null)
                throw new BadRequestException("Attendance already recorded for this student on this date.");

            if (!Enum.TryParse<AttendanceStatus>(createAttendanceDto.Status, out var status))
                throw new BadRequestException("Invalid attendance status.");

            var attendance = new Attendance
            {
                StudentId = createAttendanceDto.StudentId,
                ClassId = createAttendanceDto.ClassId,
                Date = createAttendanceDto.Date,
                Status = status,
                Remarks = createAttendanceDto.Remarks,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Attendance created with ID: {AttendanceId} for student {StudentId}", attendance.Id, createAttendanceDto.StudentId);

            return await GetAttendanceByIdAsync(attendance.Id);
        }

        public async Task<AttendanceDto> UpdateAttendanceAsync(int id, UpdateAttendanceDto updateAttendanceDto)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(id);
            if (attendance == null)
                throw new NotFoundException(nameof(Attendance), id);

            if (!Enum.TryParse<AttendanceStatus>(updateAttendanceDto.Status, out var status))
                throw new BadRequestException("Invalid attendance status.");

            attendance.Status = status;
            attendance.Remarks = updateAttendanceDto.Remarks;
            attendance.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Attendances.Update(attendance);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Attendance updated with ID: {AttendanceId}", id);

            return await GetAttendanceByIdAsync(id);
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(id);
            if (attendance == null)
                throw new NotFoundException(nameof(Attendance), id);

            _unitOfWork.Attendances.Remove(attendance);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Attendance deleted with ID: {AttendanceId}", id);
            return true;
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Class.Name.Contains(request.Search) ||
                    a.Remarks.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "date" => request.SortDescending ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date),
                "class" => request.SortDescending ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
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

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByClassAsync(int classId, SearchRequestDto request, string baseUrl)
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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Student.User.FirstName.Contains(request.Search) ||
                    a.Student.User.LastName.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(a => a.Student.User.LastName) : query.OrderBy(a => a.Student.User.LastName),
                "date" => request.SortDescending ? query.OrderByDescending(a => a.Date) : query.OrderBy(a => a.Date),
                "status" => request.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                _ => query.OrderBy(a => a.Student.User.LastName)
            };

            var totalCount = await query.CountAsync();

            var attendances = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);

            return new APIResponseDto<AttendanceDto>(attendanceDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<AttendanceDto>> GetAttendanceByDateAsync(DateTime date, SearchRequestDto request, string baseUrl)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Class)
                .Where(a => a.Date.Date == date.Date)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Student.User.FirstName.Contains(request.Search) ||
                    a.Student.User.LastName.Contains(request.Search) ||
                    a.Class.Name.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "class" => request.SortDescending ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
                "student" => request.SortDescending ? query.OrderByDescending(a => a.Student.User.LastName) : query.OrderBy(a => a.Student.User.LastName),
                "status" => request.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                _ => query.OrderBy(a => a.Class.Name).ThenBy(a => a.Student.User.LastName)
            };

            var totalCount = await query.CountAsync();

            var attendances = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);

            return new APIResponseDto<AttendanceDto>(attendanceDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int studentId, int courseId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), courseId);
            }

            // Get all classes for the course that the student is enrolled in
            var classIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.ClassId)
                .ToListAsync();

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && classIds.Contains(a.ClassId))
                .ToListAsync();

            var summary = new AttendanceSummaryDto
            {
                StudentId = studentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                CourseId = courseId,
                CourseName = course.Name,
                TotalClasses = attendances.Count,
                Present = attendances.Count(a => a.Status == AttendanceStatus.Present),
                Absent = attendances.Count(a => a.Status == AttendanceStatus.Absent),
                Late = attendances.Count(a => a.Status == AttendanceStatus.Late),
                Excused = attendances.Count(a => a.Status == AttendanceStatus.Excused)
            };

            summary.AttendancePercentage = summary.TotalClasses > 0 ?
                (decimal)(summary.Present + summary.Excused) / summary.TotalClasses * 100 : 0;

            return summary;
        }

        public async Task<bool> BulkCreateAttendanceAsync(BulkCreateAttendanceDto bulkCreateAttendanceDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == bulkCreateAttendanceDto.ClassId);
                if (classEntity == null)
                {
                    throw new NotFoundException(nameof(Class), bulkCreateAttendanceDto.ClassId);
                }

                var attendancesToAdd = new List<Attendance>();
                var today = DateTime.UtcNow;

                foreach (var attendanceItem in bulkCreateAttendanceDto.Attendances)
                {
                    // Check if student exists and is enrolled in the class
                    var enrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e => e.StudentId == attendanceItem.StudentId &&
                                                 e.ClassId == bulkCreateAttendanceDto.ClassId &&
                                                 e.Status == EnrollmentStatus.Active);

                    if (enrollment == null)
                    {
                        _logger.LogWarning("Student {StudentId} is not enrolled in class {ClassId}", attendanceItem.StudentId, bulkCreateAttendanceDto.ClassId);
                        continue;
                    }

                    // Check if attendance already exists
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == attendanceItem.StudentId &&
                                                 a.ClassId == bulkCreateAttendanceDto.ClassId &&
                                                 a.Date.Date == bulkCreateAttendanceDto.Date.Date);

                    if (existingAttendance != null)
                    {
                        // Update existing attendance
                        if (!Enum.TryParse<AttendanceStatus>(attendanceItem.Status, out var status))
                        {
                            _logger.LogWarning("Invalid attendance status for student {StudentId}", attendanceItem.StudentId);
                            continue;
                        }

                        existingAttendance.Status = status;
                        existingAttendance.Remarks = attendanceItem.Remarks;
                        _context.Attendances.Update(existingAttendance);
                    }
                    else
                    {
                        // Create new attendance
                        if (!Enum.TryParse<AttendanceStatus>(attendanceItem.Status, out var status))
                        {
                            _logger.LogWarning("Invalid attendance status for student {StudentId}", attendanceItem.StudentId);
                            continue;
                        }

                        var attendance = new Attendance
                        {
                            StudentId = attendanceItem.StudentId,
                            ClassId = bulkCreateAttendanceDto.ClassId,
                            Date = bulkCreateAttendanceDto.Date,
                            Status = status,
                            Remarks = attendanceItem.Remarks,
                            CreatedDate = today
                        };

                        attendancesToAdd.Add(attendance);
                    }
                }

                if (attendancesToAdd.Any())
                {
                    await _unitOfWork.Attendances.AddRangeAsync(attendancesToAdd);
                }

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk created/updated attendance for {Count} students in class {ClassId}",
                    attendancesToAdd.Count + bulkCreateAttendanceDto.Attendances.Count, bulkCreateAttendanceDto.ClassId);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AttendanceReportDto> GenerateAttendanceReportAsync(int classId, DateTime startDate, DateTime endDate)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
            {
                throw new NotFoundException(nameof(Class), classId);
            }

            // Get all students enrolled in the class
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            var report = new AttendanceReportDto
            {
                ClassId = classId,
                ClassName = $"{classEntity.Name} - {classEntity.Section}",
                StartDate = startDate,
                EndDate = endDate
            };

            // Get all attendance records for the date range
            var attendances = await _context.Attendances
                .Where(a => a.ClassId == classId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            var totalClassDays = await CalculateTotalClassDays(classId, startDate, endDate);

            foreach (var enrollment in enrollments)
            {
                var studentAttendances = attendances
                    .Where(a => a.StudentId == enrollment.StudentId)
                    .ToList();

                var summary = new StudentAttendanceSummaryDto
                {
                    StudentId = enrollment.StudentId,
                    StudentName = $"{enrollment.Student.User.FirstName} {enrollment.Student.User.LastName}",
                    Present = studentAttendances.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = studentAttendances.Count(a => a.Status == AttendanceStatus.Absent),
                    Late = studentAttendances.Count(a => a.Status == AttendanceStatus.Late),
                    Excused = studentAttendances.Count(a => a.Status == AttendanceStatus.Excused)
                };

                summary.AttendancePercentage = totalClassDays > 0 ?
                    (decimal)(summary.Present + summary.Excused) / totalClassDays * 100 : 0;

                report.StudentSummaries.Add(summary);
            }

            // Calculate class summary
            report.ClassSummary.TotalStudents = enrollments.Count;
            report.ClassSummary.TotalClasses = totalClassDays;
            report.ClassSummary.AverageAttendance = report.StudentSummaries.Any() ?
                report.StudentSummaries.Average(s => s.AttendancePercentage) : 0;
            report.ClassSummary.PerfectAttendance = report.StudentSummaries.Count(s => s.AttendancePercentage >= 95);
            report.ClassSummary.LowAttendance = report.StudentSummaries.Count(s => s.AttendancePercentage < 75);

            return report;
        }

        private async Task<int> CalculateTotalClassDays(int classId, DateTime startDate, DateTime endDate)
        {
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
            if (classEntity == null) return 0;

            // This is a simplified calculation
            // In a real application, you would consider holidays, breaks, etc.
            var days = classEntity.DaysOfWeek.ToCharArray();
            var totalDays = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayChar = date.DayOfWeek.ToString()[0];
                if (days.Contains(dayChar))
                {
                    totalDays++;
                }
            }

            return totalDays;
        }
    }
}