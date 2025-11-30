using SchoolManagementSystem.Application.DTOs.Shared;
using SchoolManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto request);
        Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto request);
        Task<DepartmentDto> AssignHeadOfDepartmentAsync(int departmentId, int teacherId);
        Task<bool> DeleteAsync(int id);
        Task<APIResponseDto<DepartmentDto>> GetAllAsync(SearchRequestDto request, string baseUrl);
    }
}
