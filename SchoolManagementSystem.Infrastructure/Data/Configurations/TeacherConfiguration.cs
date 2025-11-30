using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {

            builder.Property(t => t.EmployeeId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(t => t.EmployeeId)
                .IsUnique();

            builder.Property(t => t.Department)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Qualification)
                .HasMaxLength(200);

            builder.Property(t => t.Salary)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.HireDate)
                .IsRequired();

            builder.HasOne(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
        }
    }
}