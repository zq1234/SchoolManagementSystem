using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Infrastructure.Data.Interceptors
{
    public class DataChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DataChangeInterceptor> _logger;

        public DataChangeInterceptor(
            IHttpContextAccessor httpContextAccessor,
            ILogger<DataChangeInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            // Lightweight logging - only in development
            if (IsDevelopmentEnvironment())
            {
                LogChangesSummary(eventData);
            }
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // Lightweight logging - only in development
            if (IsDevelopmentEnvironment())
            {
                LogChangesSummary(eventData);
            }
            return ValueTask.FromResult(result);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            LogSaveResult(eventData, result);
            return result;
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            LogSaveResult(eventData, result);
            return ValueTask.FromResult(result);
        }

        private void LogChangesSummary(DbContextEventData eventData)
        {
            try
            {
                if (eventData.Context == null) return;

                var changedEntities = eventData.Context.ChangeTracker.Entries()
                    .Where(x => x.State == EntityState.Added ||
                               x.State == EntityState.Modified ||
                               x.State == EntityState.Deleted)
                    .ToList();

                if (!changedEntities.Any()) return;

                var contextInfo = GetContextInfo();

                // Lightweight summary logging
                _logger.LogInformation(
                    "DB Changes - Added: {AddedCount}, Modified: {ModifiedCount}, Deleted: {DeletedCount}, User: {User}",
                    changedEntities.Count(x => x.State == EntityState.Added),
                    changedEntities.Count(x => x.State == EntityState.Modified),
                    changedEntities.Count(x => x.State == EntityState.Deleted),
                    contextInfo.User
                );

                // Only log entity details in debug mode
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    foreach (var entry in changedEntities)
                    {
                        LogEntityDetails(entry, contextInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging entity changes");
            }
        }

        private void LogEntityDetails(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, ContextInfo contextInfo)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry.Entity);

            switch (entry.State)
            {
                case EntityState.Added:
                    _logger.LogDebug("CREATE {EntityName} [ID: {EntityId}] by {User}",
                        entityName, entityId, contextInfo.User);
                    break;

                case EntityState.Modified:
                    var changedProps = GetChangedProperties(entry);
                    if (changedProps.Any())
                    {
                        _logger.LogDebug("UPDATE {EntityName} [ID: {EntityId}] Changed: {Properties} by {User}",
                            entityName, entityId, string.Join(", ", changedProps), contextInfo.User);
                    }
                    break;

                case EntityState.Deleted:
                    var isSoftDelete = entry.Entity is ISoftDelete;
                    _logger.LogDebug("DELETE {EntityName} [ID: {EntityId}] Type: {Type} by {User}",
                        entityName, entityId, isSoftDelete ? "Soft" : "Hard", contextInfo.User);
                    break;
            }
        }

        private List<string> GetChangedProperties(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.Properties
                .Where(p => p.IsModified &&
                           !IsAuditProperty(p.Metadata.Name) &&
                           !IsSensitiveProperty(p.Metadata.Name))
                .Select(p => p.Metadata.Name)
                .ToList();
        }

        private void LogSaveResult(SaveChangesCompletedEventData eventData, int affectedRows)
        {
            try
            {
                var contextInfo = GetContextInfo();
                _logger.LogInformation(
                    "DB Save Completed - Rows: {AffectedRows}, User: {User}, Context: {DbContext}",
                    affectedRows,
                    contextInfo.User,
                    eventData.Context?.GetType().Name ?? "Unknown"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging save result");
            }
        }

        private ContextInfo GetContextInfo()
        {
            return new ContextInfo
            {
                User = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System",
                RequestPath = _httpContextAccessor.HttpContext?.Request?.Path.ToString() ?? "Unknown"
            };
        }

        private bool IsDevelopmentEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == "Development";
        }

        private object GetEntityId(object entity)
        {
            try
            {
                var idProperty = entity.GetType().GetProperty("Id")
                              ?? entity.GetType().GetProperty("Id",
                                  System.Reflection.BindingFlags.IgnoreCase |
                                  System.Reflection.BindingFlags.Public |
                                  System.Reflection.BindingFlags.Instance);

                return idProperty?.GetValue(entity) ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private bool IsSensitiveProperty(string propertyName)
        {
            var sensitiveProperties = new[]
            {
                "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                "RefreshToken", "Token", "Secret",
                "NormalizedEmail", "NormalizedUserName"
            };
            return sensitiveProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsAuditProperty(string propertyName)
        {
            var auditProperties = new[]
            {
                "CreatedDate", "UpdatedDate", "DeletedDate",
                "CreatedById", "UpdatedById", "DeletedById",
                "CreatedBy", "UpdatedBy", "DeletedBy"
            };
            return auditProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        private class ContextInfo
        {
            public string User { get; set; } = "System";
            public string RequestPath { get; set; } = "Unknown";
        }
    }
}