using SchoolManagementSystem.Core.Entities;
using System.Security.Claims;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}