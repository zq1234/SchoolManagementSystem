using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure.Data.Configurations
{
    public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd().UseIdentityColumn();

            // Configure properties - but NOT navigation relationships here
            builder.Property(e => e.CreatedDate).IsRequired();
            builder.Property(e => e.UpdatedDate);
            builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(e => e.DeletedDate);

            // Configure string ID fields
            builder.Property(e => e.CreatedById).HasMaxLength(450).IsRequired(false);
            builder.Property(e => e.UpdatedById).HasMaxLength(450).IsRequired(false);
            builder.Property(e => e.DeletedById).HasMaxLength(450).IsRequired(false);

           // builder.HasOne(e => e.CreatedBy)
           //.WithMany()
           //.HasForeignKey(e => e.CreatedById)
           //.OnDelete(DeleteBehavior.Restrict);

           // builder.HasOne(e => e.UpdatedBy)
           //     .WithMany()
           //     .HasForeignKey(e => e.UpdatedById)
           //     .OnDelete(DeleteBehavior.Restrict);

           // builder.HasOne(e => e.DeletedBy)
           //     .WithMany()
           //     .HasForeignKey(e => e.DeletedById)
           //     .OnDelete(DeleteBehavior.Restrict);

        }
    }
}