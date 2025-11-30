using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }

    // Soft delete properties -
    public DateTime? DeletedDate { get; set; }
    public string? DeletedById { get; set; } 

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}