namespace SchoolManagementSystem.Core.Enums
{
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        public static readonly string[] AllRoles = { SuperAdmin, Admin, Teacher, Student };
    }
}