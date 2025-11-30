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
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GradeService> _logger;

        public GradeService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<GradeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }
        public async Task<GradeDto> GetGradeByIdAsync(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                throw new NotFoundException(nameof(Grade), id);
            }

            return _mapper.Map<GradeDto>(grade);
        }
        public async Task<IEnumerable<GradeDto>> GetAllGradesAsync()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .OrderByDescending(g => g.AssessmentDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GradeDto>>(grades);
        }
        public async Task<GradeDto> CreateGradeAsync(CreateGradeDto createGradeDto)
        {
            // Check if student exists
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == createGradeDto.StudentId);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), createGradeDto.StudentId);
            }

            // Check if course exists
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == createGradeDto.CourseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), createGradeDto.CourseId);
            }

            // Check if enrollment exists
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == createGradeDto.EnrollmentId &&
                                         e.StudentId == createGradeDto.StudentId &&
                                         e.CourseId == createGradeDto.CourseId);

            if (enrollment == null)
            {
                throw new NotFoundException("Enrollment not found or does not match student and course.");
            }

            var grade = _mapper.Map<Grade>(createGradeDto);
            grade.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Grades.AddAsync(grade);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Grade created with ID: {GradeId} for student {StudentId}", grade.Id, createGradeDto.StudentId);

            return await GetGradeByIdAsync(grade.Id);
        }
        public async Task<GradeDto> UpdateGradeAsync(int id, UpdateGradeDto updateGradeDto)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(id);
            if (grade == null)
            {
                throw new NotFoundException(nameof(Grade), id);
            }

            grade.Score = updateGradeDto.Score;
            grade.TotalScore = updateGradeDto.TotalScore;
            grade.Comments = updateGradeDto.Comments;

            _unitOfWork.Grades.Update(grade);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Grade updated with ID: {GradeId}", id);

            return await GetGradeByIdAsync(id);
        }
        public async Task<bool> DeleteGradeAsync(int id)
        {
            var grade = await _unitOfWork.Grades.GetByIdAsync(id);
            if (grade == null)
            {
                throw new NotFoundException(nameof(Grade), id);
            }

            _unitOfWork.Grades.Remove(grade);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Grade deleted with ID: {GradeId}", id);
            return true;
        }
        public async Task<StudentCourseGradeDto> GetStudentCourseGradesAsync(int studentId, int courseId)
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

            var grades = await _context.Grades
                .Where(g => g.StudentId == studentId && g.CourseId == courseId)
                .OrderBy(g => g.AssessmentDate)
                .ToListAsync();

            var result = new StudentCourseGradeDto
            {
                StudentId = studentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                CourseId = courseId,
                CourseName = course.Name,
                Grades = _mapper.Map<List<GradeDto>>(grades)
            };

            if (grades.Any())
            {
                result.AverageScore = grades.Average(g => g.Percentage);
                result.FinalGrade = GradeCalculator.CalculateGrade(result.AverageScore).ToString();
                result.GPA = GradeCalculator.GetGradePoints(GradeCalculator.CalculateGrade(result.AverageScore));
            }

            return result;
        }
        public async Task<bool> BulkCreateGradesAsync(BulkCreateGradesDto bulkCreateGradesDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == bulkCreateGradesDto.CourseId);
                if (course == null)
                {
                    throw new NotFoundException(nameof(Course), bulkCreateGradesDto.CourseId);
                }

                var gradesToAdd = new List<Grade>();

                foreach (var gradeItem in bulkCreateGradesDto.Grades)
                {
                    // Check if student exists and is enrolled in the course
                    var enrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e => e.StudentId == gradeItem.StudentId &&
                                                 e.CourseId == bulkCreateGradesDto.CourseId &&
                                                 e.Status == EnrollmentStatus.Active);

                    if (enrollment == null)
                    {
                        _logger.LogWarning("Student {StudentId} is not enrolled in course {CourseId}", gradeItem.StudentId, bulkCreateGradesDto.CourseId);
                        continue;
                    }

                    var grade = new Grade
                    {
                        StudentId = gradeItem.StudentId,
                        CourseId = bulkCreateGradesDto.CourseId,
                        EnrollmentId = enrollment.Id,
                        AssessmentType = bulkCreateGradesDto.AssessmentType,
                        AssessmentName = bulkCreateGradesDto.AssessmentName,
                        Score = gradeItem.Score,
                        TotalScore = bulkCreateGradesDto.TotalScore,
                        AssessmentDate = bulkCreateGradesDto.AssessmentDate,
                        Comments = gradeItem.Comments,
                        CreatedDate = DateTime.UtcNow
                    };

                    gradesToAdd.Add(grade);
                }

                await _unitOfWork.Grades.AddRangeAsync(gradesToAdd);
                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk created {Count} grades for course {CourseId}", gradesToAdd.Count, bulkCreateGradesDto.CourseId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<APIResponseDto<GradeDto>> GetAllGradesAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(g =>
                    g.Student.User.FirstName.Contains(request.Search) ||
                    g.Student.User.LastName.Contains(request.Search) ||
                    g.Course.Name.Contains(request.Search) ||
                    g.AssessmentName.Contains(request.Search) ||
                    g.AssessmentType.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(g => g.Student.User.LastName) : query.OrderBy(g => g.Student.User.LastName),
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
        public async Task<APIResponseDto<GradeDto>> GetGradesByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(g =>
                    g.Course.Name.Contains(request.Search) ||
                    g.AssessmentName.Contains(request.Search) ||
                    g.AssessmentType.Contains(request.Search));
            }

            // Apply sorting
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
        public async Task<APIResponseDto<GradeDto>> GetGradesByCourseAsync(int courseId, SearchRequestDto request, string baseUrl)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new NotFoundException(nameof(Course), courseId);

            var query = _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Where(g => g.CourseId == courseId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(g =>
                    g.Student.User.FirstName.Contains(request.Search) ||
                    g.Student.User.LastName.Contains(request.Search) ||
                    g.AssessmentName.Contains(request.Search) ||
                    g.AssessmentType.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(g => g.Student.User.LastName) : query.OrderBy(g => g.Student.User.LastName),
                "date" => request.SortDescending ? query.OrderByDescending(g => g.AssessmentDate) : query.OrderBy(g => g.AssessmentDate),
                "score" => request.SortDescending ? query.OrderByDescending(g => g.Score) : query.OrderBy(g => g.Score),
                _ => query.OrderBy(g => g.Student.User.LastName)
            };

            var totalCount = await query.CountAsync();

            var grades = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);

            return new APIResponseDto<GradeDto>(gradeDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }
    }
}