using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Infrastructure.Data.Configurations;
using System.Reflection;

namespace SchoolManagementSystem.Infrastructure.Data
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyAllConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
        {
            // Apply all IEntityTypeConfiguration<T> implementations
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

            // Automatically apply BaseEntityConfiguration<T> for all entities inheriting BaseEntity
            var baseEntityType = typeof(BaseEntity);
            var allEntities = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseEntityType.IsAssignableFrom(t));

            foreach (var entityType in allEntities)
            {
                var configType = typeof(BaseEntityConfiguration<>).MakeGenericType(entityType);
                dynamic configInstance = Activator.CreateInstance(configType);

                // Avoid duplicate registration
                if (!modelBuilder.Model.GetEntityTypes().Any(e => e.ClrType == entityType && e.GetAnnotations().Any(a => a.Name == "BaseEntityApplied")))
                {
                    modelBuilder.ApplyConfiguration(configInstance);
                    modelBuilder.Entity(entityType).HasAnnotation("BaseEntityApplied", true);
                }
            }
        }
    }
}
