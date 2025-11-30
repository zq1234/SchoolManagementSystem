using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(c => c.Code)
                .IsUnique();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.Credits)
                .IsRequired();

            builder.Property(c => c.Duration)
                .IsRequired();

            builder.Property(c => c.Fee)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.Department)
         .WithMany(d => d.Courses)
         .HasForeignKey(c => c.DepartmentId)
         .OnDelete(DeleteBehavior.SetNull);
        }
    }
}