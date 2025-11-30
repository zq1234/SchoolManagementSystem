using AutoMapper;
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

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<SubmissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<SubmissionDto> GetSubmissionByIdAsync(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Include(s => s.GradedByTeacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                throw new NotFoundException(nameof(Submission), id);

            return _mapper.Map<SubmissionDto>(submission);
        }

        public async Task<SubmissionDto> SubmitAssignmentAsync(CreateSubmissionDto createSubmissionDto, int studentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == createSubmissionDto.AssignmentId);

            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), createSubmissionDto.AssignmentId);

            // Check if student is enrolled in the class
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                         e.ClassId == assignment.ClassId &&
                                         e.Status == EnrollmentStatus.Active);

            if (enrollment == null)
                throw new BadRequestException("Student is not enrolled in this class.");

            // Check if already submitted
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == createSubmissionDto.AssignmentId &&
                                         s.StudentId == studentId);

            if (existingSubmission != null)
                throw new BadRequestException("Assignment already submitted.");

            // Save file
            var folderPath = $"uploads/submissions/assignment-{assignment.Id}/student-{studentId}";
            var fileUrl = await _fileService.SaveFileAsync(createSubmissionDto.File, folderPath);

            var submission = new Submission
            {
                AssignmentId = createSubmissionDto.AssignmentId,
                StudentId = studentId,
                SubmittedDate = DateTime.UtcNow,
                FileUrl = fileUrl,
                Remarks = createSubmissionDto.Remarks,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Submissions.AddAsync(submission);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Submission created with ID: {SubmissionId} for assignment {AssignmentId} by student {StudentId}",
                submission.Id, createSubmissionDto.AssignmentId, studentId);

            return await GetSubmissionByIdAsync(submission.Id);
        }

        public async Task<SubmissionDto> UpdateSubmissionAsync(int id, UpdateSubmissionDto updateSubmissionDto)
        {
            var submission = await _unitOfWork.Submissions.GetByIdAsync(id);
            if (submission == null)
                throw new NotFoundException(nameof(Submission), id);

            submission.Remarks = updateSubmissionDto.Remarks;
            submission.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Submissions.Update(submission);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Submission updated with ID: {SubmissionId}", id);

            return await GetSubmissionByIdAsync(id);
        }

        public async Task<SubmissionDto> GradeSubmissionAsync(int id, GradeSubmissionDto gradeSubmissionDto, int teacherId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                throw new NotFoundException(nameof(Submission), id);

            // Verify teacher has access to this assignment
            var teacherAssignment = await _context.Assignments
                .AnyAsync(a => a.Id == submission.AssignmentId && a.CreatedByTeacherId == teacherId);

            if (!teacherAssignment)
                throw new UnauthorizedException("You are not authorized to grade this submission.");

            submission.Grade = gradeSubmissionDto.Grade;
            submission.GradedByTeacherId = teacherId;
            submission.Remarks = gradeSubmissionDto.Remarks ?? submission.Remarks;
            submission.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Submissions.Update(submission);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Submission graded with ID: {SubmissionId} by teacher {TeacherId}", id, teacherId);

            return await GetSubmissionByIdAsync(id);
        }

        public async Task<bool> DeleteSubmissionAsync(int id)
        {
            var submission = await _unitOfWork.Submissions.GetByIdAsync(id);
            if (submission == null)
                throw new NotFoundException(nameof(Submission), id);

            // Delete file
            if (!string.IsNullOrEmpty(submission.FileUrl))
                 _fileService.DeleteFile(submission.FileUrl);

            _unitOfWork.Submissions.Remove(submission);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Submission deleted with ID: {SubmissionId}", id);
            return true;
        }

        public async Task<APIResponseDto<SubmissionDto>> GetSubmissionsByAssignmentAsync(int assignmentId, SearchRequestDto request, string baseUrl)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), assignmentId);

            var query = _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Include(s => s.GradedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.AssignmentId == assignmentId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(s =>
                    s.Student.User.FirstName.Contains(request.Search) ||
                    s.Student.User.LastName.Contains(request.Search) ||
                    s.Remarks.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "student" => request.SortDescending ? query.OrderByDescending(s => s.Student.User.LastName) : query.OrderBy(s => s.Student.User.LastName),
                "date" => request.SortDescending ? query.OrderByDescending(s => s.SubmittedDate) : query.OrderBy(s => s.SubmittedDate),
                "grade" => request.SortDescending ? query.OrderByDescending(s => s.Grade) : query.OrderBy(s => s.Grade),
                _ => query.OrderBy(s => s.Student.User.LastName)
            };

            var totalCount = await query.CountAsync();

            var submissions = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var submissionDtos = _mapper.Map<IEnumerable<SubmissionDto>>(submissions);

            return new APIResponseDto<SubmissionDto>(submissionDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<APIResponseDto<SubmissionDto>> GetSubmissionsByStudentAsync(int studentId, SearchRequestDto request, string baseUrl)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                throw new NotFoundException(nameof(Student), studentId);

            var query = _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User)
                .Include(s => s.GradedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.StudentId == studentId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(s =>
                    s.Assignment.Title.Contains(request.Search) ||
                    s.Remarks.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "assignment" => request.SortDescending ? query.OrderByDescending(s => s.Assignment.Title) : query.OrderBy(s => s.Assignment.Title),
                "date" => request.SortDescending ? query.OrderByDescending(s => s.SubmittedDate) : query.OrderBy(s => s.SubmittedDate),
                "grade" => request.SortDescending ? query.OrderByDescending(s => s.Grade) : query.OrderBy(s => s.Grade),
                _ => query.OrderByDescending(s => s.SubmittedDate)
            };

            var totalCount = await query.CountAsync();

            var submissions = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var submissionDtos = _mapper.Map<IEnumerable<SubmissionDto>>(submissions);

            return new APIResponseDto<SubmissionDto>(submissionDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<SubmissionStatsDto> GetSubmissionStatsAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), assignmentId);

            var totalStudents = await _context.Enrollments
                .CountAsync(e => e.ClassId == assignment.ClassId && e.Status == EnrollmentStatus.Active);

            var submissions = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            var gradedSubmissions = submissions.Count(s => s.Grade != null);

            return new SubmissionStatsDto
            {
                AssignmentId = assignmentId,
                AssignmentTitle = assignment.Title,
                TotalStudents = totalStudents,
                TotalSubmissions = submissions.Count,
                GradedSubmissions = gradedSubmissions,
                PendingSubmissions = submissions.Count - gradedSubmissions,
                SubmissionRate = totalStudents > 0 ? (decimal)submissions.Count / totalStudents * 100 : 0
            };
        }

        
    }
}