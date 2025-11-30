using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {

        builder.Property(g => g.AssessmentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.AssessmentName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Score)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(g => g.TotalScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(g => g.AssessmentDate)
            .IsRequired();

        builder.Property(g => g.Comments)
            .HasMaxLength(500);

        // Ignore computed properties
        builder.Ignore(g => g.Percentage);
        builder.Ignore(g => g.GradeLetter);

        builder.HasIndex(g => new { g.StudentId, g.CourseId, g.AssessmentType, g.AssessmentName })
            .IsUnique();

        builder.HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Course)
            .WithMany()
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Enrollment)
            .WithMany(e => e.Grades)
            .HasForeignKey(g => g.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
