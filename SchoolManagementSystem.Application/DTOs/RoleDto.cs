namespace SchoolManagementSystem.Application.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }

   
}