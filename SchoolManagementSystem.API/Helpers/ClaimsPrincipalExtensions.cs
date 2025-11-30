 
    using System.Security.Claims;

    namespace SchoolManagementSystem.API.Extensions
    {
        public static class ClaimsPrincipalExtensions
        {
            public static int GetStudentId(this ClaimsPrincipal principal)
            {
                var studentIdClaim = principal.FindFirst("StudentId")?.Value;
                if (int.TryParse(studentIdClaim, out int studentId))
                    return studentId;
                return 0;
            }

            public static int GetTeacherId(this ClaimsPrincipal principal)
            {
                var teacherIdClaim = principal.FindFirst("TeacherId")?.Value;
                if (int.TryParse(teacherIdClaim, out int teacherId))
                    return teacherId;
                return 0;
            }

            public static string GetUserId(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            }

            public static string GetUserRole(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }

            public static string GetUserEmail(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            }

            public static string GetUserName(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            }

            public static string GetFullName(this ClaimsPrincipal principal)
            {
                return principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            }
        }
    }

