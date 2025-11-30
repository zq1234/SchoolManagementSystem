using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Exceptions;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<AssignmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<AssignmentDto> GetAssignmentByIdAsync(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), id);

            return _mapper.Map<AssignmentDto>(assignment);
        }

        public async Task<APIResponseDto<AssignmentDto>> GetAllAssignmentsAsync(SearchRequestDto request, string baseUrl)
        {
            var query = _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.IsActive == true)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.Search) ||
                    a.Description.Contains(request.Search));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "duedate" => request.SortDescending ? query.OrderByDescending(a => a.DueDate) : query.OrderBy(a => a.DueDate),
                _ => query.OrderByDescending(a => a.DueDate) // Default sorting
            };

            var totalCount = await query.CountAsync();

            var assignments = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            var assignmentDtos = _mapper.Map<IEnumerable<AssignmentDto>>(assignments);

            return new APIResponseDto<AssignmentDto>(assignmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
        }

        public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentDto createAssignmentDto, int teacherId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == createAssignmentDto.ClassId);

            if (classEntity == null)
                throw new NotFoundException(nameof(Class), createAssignmentDto.ClassId);

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException(nameof(Teacher), teacherId);

            var assignment = new Assignment
            {
                Title = createAssignmentDto.Title,
                Description = createAssignmentDto.Description,
                DueDate = createAssignmentDto.DueDate,
                ClassId = createAssignmentDto.ClassId,
                CreatedByTeacherId = teacherId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Assignments.AddAsync(assignment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Assignment created with ID: {AssignmentId} by teacher {TeacherId}", assignment.Id, teacherId);

            return await GetAssignmentByIdAsync(assignment.Id);
        }

        public async Task<AssignmentDto> UpdateAssignmentAsync(int id, UpdateAssignmentDto updateAssignmentDto)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), id);

            assignment.Title = updateAssignmentDto.Title;
            assignment.Description = updateAssignmentDto.Description;
            assignment.DueDate = updateAssignmentDto.DueDate;
            assignment.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Assignments.Update(assignment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Assignment updated with ID: {AssignmentId}", id);

            return await GetAssignmentByIdAsync(id);
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), id);

            // Check if there are submissions
            var hasSubmissions = await _context.Submissions.AnyAsync(s => s.AssignmentId == id);
            if (hasSubmissions)
                throw new BadRequestException("Cannot delete assignment with existing submissions.");

            assignment.IsActive = false;
            assignment.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Assignments.Update(assignment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Assignment deleted with ID: {AssignmentId}", id);
            return true;
        }

        public async Task<APIResponseDto<AssignmentDto>> GetAssignmentsByClassAsync(int classId, SearchRequestDto request, string baseUrl)
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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.Search) ||
                    a.Description.Contains(request.Search));
            }

            // Apply sorting
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

        public async Task<APIResponseDto<AssignmentDto>> GetAssignmentsByTeacherAsync(int teacherId, SearchRequestDto request, string baseUrl)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException(nameof(Teacher), teacherId);

            var query = _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.CreatedByTeacherId == teacherId && a.IsActive == true)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.Search) ||
                    a.Description.Contains(request.Search));
            }

            // Apply sorting
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

        public async Task<AssignmentDetailDto> GetAssignmentDetailAsync(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.CreatedByTeacher)
                    .ThenInclude(t => t.User)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                throw new NotFoundException(nameof(Assignment), id);

            var assignmentDetail = _mapper.Map<AssignmentDetailDto>(assignment);
            assignmentDetail.TotalSubmissions = assignment.Submissions.Count;
            assignmentDetail.GradedSubmissions = assignment.Submissions.Count(s => s.Grade != null);

            return assignmentDetail;
        }
    }
}