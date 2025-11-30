using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.API.Extensions;
using SchoolManagementSystem.API.Helpers;
using SchoolManagementSystem.API.Middleware;
using SchoolManagementSystem.Application.Mappings;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Configure Serilog
// ---------------------------
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SchoolManagementSystem")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
    }
});

try
{
    Log.Information("Starting School Management System API...");

    // ---------------------------
    // Services Registration
    // ---------------------------
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddIdentityServices();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddValidationServices();
    builder.Services.AddMediatorServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddCorsPolicy();
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddControllers();
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.ReportApiVersions = true;
    });

    builder.Services.AddFluentValidationAutoValidation();

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("Database");

    var app = builder.Build();

    // ---------------------------
    // Middleware & Pipeline
    // ---------------------------
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseMiddleware<GlobalExceptionHandler>();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAll");

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // ---------------------------
    // Apply Migrations + Seed Data
    // ---------------------------
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        Log.Information("Applying migrations & seeding database...");

        await DatabaseInitializer.ApplyMigrationsAndSeedAsync(services);

        Log.Information("Database migrations and seeding completed.");
    }

    // ---------------------------
    // Start Application
    // ---------------------------
    Log.Information("School Management System API started successfully");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
