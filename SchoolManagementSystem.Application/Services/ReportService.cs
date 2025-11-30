using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Enums;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StudentReportDto> GenerateStudentReportAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Grades)
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Class)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }

            var report = new StudentReportDto
            {
                StudentId = student.Id,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentIdNumber = student.StudentId,
                EnrollmentDate = student.EnrollmentDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Calculate course grades and attendance
            foreach (var enrollment in student.Enrollments.Where(e =>
                e.Status == EnrollmentStatus.Completed || e.Status == EnrollmentStatus.Active))
            {
                var courseGrades = student.Grades
                    .Where(g => g.CourseId == enrollment.CourseId)
                    .ToList();

                // Fixed: Added null checks for navigation properties
                var courseAttendances = student.Attendances
                    .Where(a => a.Class != null && a.Class.CourseId == enrollment.CourseId)
                    .ToList();

                var averageGrade = courseGrades.Any() ? courseGrades.Average(g => g.Percentage) : 0;
                var attendancePercentage = courseAttendances.Any() ?
                    (decimal)courseAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Excused) / courseAttendances.Count * 100 : 0;

                report.Courses.Add(new CourseReportItemDto
                {
                    CourseId = enrollment.CourseId,
                    CourseCode = enrollment.Course?.Code ?? "N/A",
                    CourseName = enrollment.Course?.Name ?? "Unknown Course",
                    Grade = averageGrade,
                    GradeLetter = GradeCalculator.CalculateGrade(averageGrade).ToString(),
                    Attendance = attendancePercentage
                });
            }

            report.OverallGPA = CalculateOverallGPA(student.Grades);
            report.OverallAttendance = CalculateOverallAttendance(student.Attendances);

            _logger.LogInformation("Generated student report for student ID: {StudentId}", studentId);
            return report;
        }

        public async Task<TeacherReportDto> GenerateTeacherReportAsync(int teacherId)
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

            var report = new TeacherReportDto
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                EmployeeId = teacher.EmployeeId,
                Department = teacher.Department,
                GeneratedAt = DateTime.UtcNow
            };

            // Add course information
            foreach (var course in teacher.Courses)
            {
                var studentCount = await _context.Enrollments
                    .CountAsync(e => e.CourseId == course.Id && e.Status == EnrollmentStatus.Active);

                // FIXED: Load grades into memory first to avoid query translation issues
                var grades = await _context.Grades
                    .Where(g => g.CourseId == course.Id)
                    .Select(g => g.Percentage)
                    .ToListAsync();

                var averageGrade = grades.Any() ? grades.Average() : 0;

                report.Courses.Add(new CourseReportItemDto
                {
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    Grade = (decimal)averageGrade,
                    GradeLetter = GradeCalculator.CalculateGrade((decimal)averageGrade).ToString(),
                    Attendance = await CalculateCourseAttendance(course.Id)
                });
            }

            // Fixed: Added null checks for navigation properties
            var studentIdsFromClasses = await _context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.Class != null && e.Class.TeacherId == teacherId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            var studentIdsFromCourses = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.Course != null && e.Course.TeacherId == teacherId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            // Combine and count unique students
            var allStudentIds = studentIdsFromClasses.Union(studentIdsFromCourses).Distinct();
            report.TotalStudents = allStudentIds.Count();

            report.TotalClasses = teacher.Classes.Count;

            _logger.LogInformation("Generated teacher report for teacher ID: {TeacherId}", teacherId);
            return report;
        }

        public async Task<CourseReportDto> GenerateCourseReportAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                throw new NotFoundException(nameof(Course), courseId);
            }

            var report = new CourseReportDto
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                TeacherName = course.Teacher != null ?
                    $"{course.Teacher.User.FirstName} {course.Teacher.User.LastName}" : "Not Assigned",
                GeneratedAt = DateTime.UtcNow
            };

            // Get all enrolled students and their grades
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Grades)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            report.TotalStudents = enrollments.Count;

            foreach (var enrollment in enrollments)
            {
                var averageGrade = enrollment.Grades.Any() ?
                    enrollment.Grades.Average(g => g.Percentage) : 0;

                report.StudentGrades.Add(new StudentGradeDto
                {
                    StudentId = enrollment.StudentId,
                    StudentName = $"{enrollment.Student.User.FirstName} {enrollment.Student.User.LastName}",
                    Grade = averageGrade,
                    GradeLetter = GradeCalculator.CalculateGrade(averageGrade).ToString()
                });
            }

            report.AverageGrade = report.StudentGrades.Any() ?
                report.StudentGrades.Average(sg => sg.Grade) : 0;
            report.AverageAttendance = await CalculateCourseAttendance(courseId);

            _logger.LogInformation("Generated course report for course ID: {CourseId}", courseId);
            return report;
        }

        public async Task<ClassReportDto> GenerateClassReportAsync(int classId)
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

            var report = new ClassReportDto
            {
                ClassId = classEntity.Id,
                ClassName = $"{classEntity.Name} - {classEntity.Section}",
                CourseName = classEntity.Course?.Name ?? "Unknown Course",
                TeacherName = classEntity.Teacher != null ?
                    $"{classEntity.Teacher.User.FirstName} {classEntity.Teacher.User.LastName}" : "Not Assigned",
                GeneratedAt = DateTime.UtcNow
            };

            // Get student attendance
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            report.TotalStudents = enrollments.Count;

            foreach (var enrollment in enrollments)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == enrollment.StudentId && a.ClassId == classId)
                    .ToListAsync();

                var attendancePercentage = attendances.Any() ?
                    (decimal)attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Excused) / attendances.Count * 100 : 0;

                report.StudentAttendances.Add(new StudentAttendanceDto
                {
                    StudentId = enrollment.StudentId,
                    StudentName = $"{enrollment.Student.User.FirstName} {enrollment.Student.User.LastName}",
                    AttendancePercentage = attendancePercentage
                });
            }

            _logger.LogInformation("Generated class report for class ID: {ClassId}", classId);
            return report;
        }

        public async Task<SchoolReportDto> GenerateSchoolReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new SchoolReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Basic statistics with null checks
            report.TotalStudents = await _context.Students
                .CountAsync(s => s.User != null && s.User.IsActive);

            report.TotalTeachers = await _context.Teachers
                .CountAsync(t => t.User != null && t.User.IsActive);

            report.TotalCourses = await _context.Courses
                .CountAsync(c => c.IsActive == true);

            report.TotalClasses = await _context.Classes.CountAsync();

            // Overall attendance (simplified)
            var totalAttendances = await _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            report.OverallAttendance = totalAttendances.Any() ?
                (decimal)totalAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Excused) / totalAttendances.Count * 100 : 0;

            // Overall GPA (simplified) - FIXED: Load into memory first
            var allGrades = await _context.Grades
                .Where(g => g.AssessmentDate >= startDate && g.AssessmentDate <= endDate)
                .Select(g => g.Percentage)
                .ToListAsync();

            var gradePoints = allGrades.Select(g => GradeCalculator.GetGradePoints(GradeCalculator.CalculateGrade(g)));
            report.OverallGPA = gradePoints.Any() ? gradePoints.Average() : 0;

            // Top courses by student count with null checks
            report.TopCourses = await _context.Courses
                .Where(c => c.IsActive == true)
                .Select(c => new CourseSummaryDto
                {
                    CourseId = c.Id,
                    CourseName = c.Name,
                    StudentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                    AverageGrade = (decimal)(c.Enrollments
                        .SelectMany(e => e.Grades)
                        .Average(g => (double?)g.Percentage) ?? 0)
                })
                .OrderByDescending(c => c.StudentCount)
                .Take(10)
                .ToListAsync();

            // Top teachers by course count with null checks
            report.TopTeachers = await _context.Teachers
                .Where(t => t.User != null && t.User.IsActive)
                .Select(t => new TeacherSummaryDto
                {
                    TeacherId = t.Id,
                    TeacherName = $"{t.User.FirstName} {t.User.LastName}",
                    CourseCount = t.Courses.Count,
                    StudentCount = t.Classes.Sum(c => c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active))
                })
                .OrderByDescending(t => t.CourseCount)
                .Take(10)
                .ToListAsync();

            _logger.LogInformation("Generated school report for period {StartDate} to {EndDate}", startDate, endDate);
            return report;
        }

        public async Task<FinancialReportDto> GenerateFinancialReportAsync(int academicYear)
        {
            var report = new FinancialReportDto
            {
                AcademicYear = academicYear,
                GeneratedAt = DateTime.UtcNow
            };

            // Calculate revenue from course fees with null checks
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.EnrollmentDate.Year == academicYear && e.Course != null)
                .ToListAsync();

            report.TotalRevenue = enrollments.Sum(e => e.Course?.Fee ?? 0);

            // Calculate expenses (this would typically come from an expenses table)
            // For now, using placeholder values
            report.ExpenseItems = new List<ExpenseItemDto>
            {
                new() { Description = "Teacher Salaries", Amount = 500000 },
                new() { Description = "Facility Maintenance", Amount = 75000 },
                new() { Description = "Administrative Costs", Amount = 50000 },
                new() { Description = "Educational Materials", Amount = 25000 }
            };

            report.TotalExpenses = report.ExpenseItems.Sum(e => e.Amount);
            report.NetIncome = report.TotalRevenue - report.TotalExpenses;

            // Revenue items with null checks
            report.RevenueItems = enrollments
                .Where(e => e.Course != null)
                .GroupBy(e => e.Course.Name)
                .Select(g => new RevenueItemDto
                {
                    Description = $"{g.Key} Course Fees",
                    Amount = g.Sum(e => e.Course?.Fee ?? 0)
                })
                .ToList();

            _logger.LogInformation("Generated financial report for academic year {AcademicYear}", academicYear);
            return report;
        }

        private static decimal CalculateOverallGPA(ICollection<Grade> grades)
        {
            if (!grades.Any()) return 0;

            var courseGrades = grades
                .GroupBy(g => g.CourseId)
                .Select(g => new
                {
                    Average = g.Average(x => x.Percentage),
                    Credits = 3 // This should come from course entity
                })
                .ToList();

            var totalGradePoints = courseGrades.Sum(cg =>
                GradeCalculator.GetGradePoints(GradeCalculator.CalculateGrade(cg.Average)) * cg.Credits);
            var totalCredits = courseGrades.Sum(cg => cg.Credits);

            return totalCredits > 0 ? totalGradePoints / totalCredits : 0;
        }

        private static decimal CalculateOverallAttendance(ICollection<Attendance> attendances)
        {
            if (!attendances.Any()) return 0;

            var totalClasses = attendances.Count;
            var presentClasses = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Excused);

            return (decimal)presentClasses / totalClasses * 100;
        }

        private async Task<decimal> CalculateCourseAttendance(int courseId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Class)
                .Where(a => a.Class != null && a.Class.CourseId == courseId)
                .ToListAsync();

            if (!attendances.Any()) return 0;

            var totalClasses = attendances.Count;
            var presentClasses = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Excused);

            return (decimal)presentClasses / totalClasses * 100;
        }
    }
}