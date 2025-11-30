using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Data.Configurations;
using System.Reflection;
using System.Security.Claims;

namespace SchoolManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<SeedHistory> SeedHistories { get; set; }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyAllConfigurations(Assembly.GetExecutingAssembly());
            ApplySoftDeleteQueryFilter(builder);
        }

        private static void ApplySoftDeleteQueryFilter(ModelBuilder builder)
        {
            var entityTypes = builder.Model.GetEntityTypes()
                .Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType));

            foreach (var entityType in entityTypes)
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsActive));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(true));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        private void SetAuditFields()
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentTime = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = currentTime;
                        entry.Entity.CreatedById = currentUserId;
                        entry.Entity.IsActive = true;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = currentTime;
                        entry.Entity.UpdatedById = currentUserId;

                        if (entry.Entity is ISoftDelete softDeleteEntity && !softDeleteEntity.IsActive)
                        {
                            if (entry.Entity.DeletedDate == null)
                            {
                                entry.Entity.DeletedDate = currentTime;
                                entry.Entity.DeletedById = currentUserId;
                            }
                        }
                        break;

                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete)
                        {
                            entry.State = EntityState.Modified;
                            entry.Entity.IsActive = false;
                            entry.Entity.DeletedDate = currentTime;
                            entry.Entity.DeletedById = currentUserId;
                        }
                        break;
                }
            }
        }

        public IQueryable<T> IgnoreSoftDeleteFilter<T>() where T : class
        {
            return Set<T>().IgnoreQueryFilters();
        }

        public IQueryable<T> GetAllIncludingInactive<T>() where T : BaseEntity
        {
            return Set<T>().IgnoreQueryFilters();
        }
    }
}