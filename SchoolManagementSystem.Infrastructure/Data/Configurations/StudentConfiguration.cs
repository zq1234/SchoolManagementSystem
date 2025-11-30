using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {

            builder.Property(s => s.StudentId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(s => s.StudentId)
                .IsUnique();

            builder.Property(s => s.DateOfBirth)
                .IsRequired();

            builder.Property(s => s.EnrollmentDate)
                .IsRequired();

            builder.Property(s => s.Address)
                .HasMaxLength(500);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(15);

            builder.Property(s => s.PhotoUrl)
                .HasMaxLength(500);

            builder.HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

           
        }
    }
}