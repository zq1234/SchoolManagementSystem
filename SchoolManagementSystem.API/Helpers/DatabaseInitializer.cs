using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Infrastructure.Data;
using Serilog;

namespace SchoolManagementSystem.API.Helpers
{
    public static class DatabaseInitializer
    {
        public static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var context = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                Log.Information("Starting database migration...");

                await context.Database.MigrateAsync();

                Log.Information("Database migration completed successfully.");

                // Run initial seed (ONE-TIME)
                await RunSeedAsync(context, "INITIAL-SEED", async () =>
                {
                    Log.Information("Running INITIAL-SEED data population...");
                    await SeedData.Initialize(context, userManager, roleManager);
                    Log.Information("INITIAL-SEED completed successfully.");
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during migrations or seeding.");
            }
        }

        // Prevents a seed block from running more than once
        private static async Task RunSeedAsync(ApplicationDbContext context, string seedKey, Func<Task> action)
        {
            Log.Information("Checking seed status for key: {SeedKey}", seedKey);

            bool alreadySeeded = await context.SeedHistories.AnyAsync(x => x.SeedKey == seedKey);

            if (alreadySeeded)
            {
                Log.Information("Skipping seed '{SeedKey}' — already executed before.", seedKey);
                return;
            }

            try
            {
                Log.Information("Executing seed '{SeedKey}'...", seedKey);

                await action();

                context.SeedHistories.Add(new SeedHistory
                {
                    SeedKey = seedKey,
                    CreatedOn = DateTime.UtcNow
                });

                await context.SaveChangesAsync();

                Log.Information("Seed '{SeedKey}' saved to SeedHistory table.", seedKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Seed '{SeedKey}' failed.", seedKey);
            }
        }
    }
}
