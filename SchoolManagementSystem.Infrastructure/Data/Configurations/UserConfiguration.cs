using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Configure properties that are NOT in IdentityUser
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PhotoUrl)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedDate)
                .IsRequired();

            builder.Property(u => u.UpdatedDate);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.RefreshToken)
                .HasMaxLength(500);

            builder.Property(u => u.RefreshTokenExpiry);

            // Soft delete properties -  
            builder.Property(u => u.DeletedDate);

            builder.Property(u => u.DeletedById)
                .HasMaxLength(450); 

            // Indexes for custom properties
            builder.HasIndex(u => u.IsActive);
            builder.HasIndex(u => new { u.FirstName, u.LastName });
            builder.HasIndex(u => u.DeletedDate);  

            // Ignore the computed FullName property since it's [NotMapped]
            builder.Ignore(u => u.FullName);

            // Configure relationships
            builder.HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}