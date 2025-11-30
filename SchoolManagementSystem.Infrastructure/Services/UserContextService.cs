using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly ApplicationDbContext _context;

        public UserContextService(ApplicationDbContext context)
        {
            _context = context;
        }

        public (int studentId, int teacherId) GetUserProfileIds(string userId)
        {
            var studentId = _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefault();

            var teacherId = _context.Teachers
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .FirstOrDefault();

            return (studentId, teacherId);
        }

        public async Task PopulateUserClaims(User user, List<Claim> claims)
        {
            var (studentId, teacherId) = GetUserProfileIds(user.Id);

            if (studentId > 0)
            {
                claims.Add(new Claim("StudentId", studentId.ToString()));
                var student = await _context.Students.FindAsync(studentId);
                if (student != null)
                {
                    claims.Add(new Claim("StudentNumber", student.StudentId));
                }
            }

            if (teacherId > 0)
            {
                claims.Add(new Claim("TeacherId", teacherId.ToString()));
                var teacher = await _context.Teachers.FindAsync(teacherId);
                if (teacher != null)
                {
                    claims.Add(new Claim("EmployeeId", teacher.EmployeeId));
                }
            }
        }
    }
}
