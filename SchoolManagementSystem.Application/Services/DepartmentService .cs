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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<DepartmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<DepartmentDto> GetByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                throw new NotFoundException(nameof(Department), id);

            return _mapper.Map<DepartmentDto>(department);
        }

       
        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto request)
        {
            // Validate HOD exists
            var teacherExists = await _context.Teachers.AnyAsync(t => t.Id == request.HeadOfDepartmentId);
            if (!teacherExists)
                throw new BadRequestException("Head of Department does not exist.");

            // Check duplicate
            var exists = await _context.Departments
                .AnyAsync(d => d.Name == request.Name);

            if (exists)
                throw new BadRequestException("A department with this name already exists.");

            var entity = _mapper.Map<Department>(request);
            entity.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Departments.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Department created with ID: {DepartmentId}", entity.Id);

            return await GetByIdAsync(entity.Id);
        }

        public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto request)
        {
            var entity = await _unitOfWork.Departments.GetByIdAsync(id);
            if (entity == null)
                throw new NotFoundException(nameof(Department), id);

            // Check duplicate name
            var exists = await _context.Departments
                .AnyAsync(d =>
                    d.Name == request.Name &&
                    d.Id != id);

            if (exists)
                throw new BadRequestException("A department with this name already exists.");

            _mapper.Map(request, entity);
            entity.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Departments.Update(entity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Department updated with ID: {DepartmentId}", id);

            return await GetByIdAsync(id);
        }
        public async Task<DepartmentDto> AssignHeadOfDepartmentAsync(int departmentId, int teacherId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == departmentId);

            if (department == null)
                throw new NotFoundException($"Department with ID {departmentId} not found.");

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(x => x.Id == teacherId);

            if (teacher == null)
                throw new NotFoundException($"Teacher with ID {teacherId} not found.");

            // Optional rule: Only one HOD per teacher
            var teacherAlreadyHod = await _context.Departments
                 .AnyAsync(x => x.HeadOfDepartmentId == teacherId && x.Id != departmentId);

            if (teacherAlreadyHod)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "HeadOfDepartmentId", new[] { "This teacher is already Head of another department." } }
                };
                throw new ValidationException(errors);
            }
            department.HeadOfDepartmentId = teacherId;
            department.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<DepartmentDto>(department);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);

            if (department == null)
                throw new NotFoundException(nameof(Department), id);

            _unitOfWork.Departments.Remove(department);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Department deleted with ID: {DepartmentId}", id);

            return true;
        }
    
            public async Task<APIResponseDto<DepartmentDto>> GetAllAsync(SearchRequestDto request, string baseUrl)
            {
                var query = _context.Departments
                    .Include(d => d.HeadOfDepartment)
                        .ThenInclude(t => t.User)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    query = query.Where(d =>
                        d.Name.Contains(request.Search) ||
                        d.Description.Contains(request.Search) ||
                        (d.HeadOfDepartment != null && d.HeadOfDepartment.User.FirstName.Contains(request.Search)) ||
                        (d.HeadOfDepartment != null && d.HeadOfDepartment.User.LastName.Contains(request.Search)));
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
                    "hod" => request.SortDescending ? query.OrderByDescending(d => d.HeadOfDepartment.User.LastName) : query.OrderBy(d => d.HeadOfDepartment.User.LastName),
                    _ => query.OrderBy(d => d.Name)
                };

                var totalCount = await query.CountAsync();

                var departments = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .ToListAsync();

                var departmentDtos = _mapper.Map<IEnumerable<DepartmentDto>>(departments);

                return new APIResponseDto<DepartmentDto>(departmentDtos, request.Page, request.PageSize, totalCount, baseUrl);
            }

           

          
        }
    }