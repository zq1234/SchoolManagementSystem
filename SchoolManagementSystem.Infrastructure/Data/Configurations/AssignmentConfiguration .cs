using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.Property(a => a.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.Description)
                .HasMaxLength(1000);

            builder.Property(a => a.DueDate)
                .IsRequired();

            // Relationships -  
            builder.HasOne(a => a.Class)
             .WithMany(c => c.Assignments) 
             .HasForeignKey(a => a.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.CreatedByTeacher)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better performance
            builder.HasIndex(a => a.ClassId);
            builder.HasIndex(a => a.CreatedByTeacherId);
            builder.HasIndex(a => a.DueDate);
        }
    }
}