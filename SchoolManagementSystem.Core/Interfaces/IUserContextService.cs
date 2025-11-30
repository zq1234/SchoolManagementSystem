using SchoolManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IUserContextService
    {
        (int studentId, int teacherId) GetUserProfileIds(string userId);
        Task PopulateUserClaims(User user, List<Claim> claims);
    }
}
