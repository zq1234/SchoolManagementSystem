using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Infrastructure.Repositories;
using SchoolManagementSystem.Infrastructure.Services;

namespace SchoolManagementSystem.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = configuration.GetValue<long?>("CacheSettings:SizeLimit") ?? 100;
            });

            // Infrastructure
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IDateTimeService, DateTimeService>();
            services.AddScoped<ICacheService, CacheService>();

            // Business Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAssignmentService, AssignmentService>(); 
            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserContextService, UserContextService>();

            // Caching Decorators
            services.Decorate<IUserService, CachingUserService>();
            services.Decorate<IStudentService, CachingStudentService>();
            services.Decorate<ITeacherService, CachingTeacherService>();
            services.Decorate<ICourseService, CachingCourseService>();
            services.Decorate<IClassService, CachingClassService>();
            services.Decorate<IEnrollmentService, CachingEnrollmentService>(); 
            services.Decorate<IGradeService, CachingGradeService>(); 
            services.Decorate<IAttendanceService, CachingAttendanceService>();
            services.Decorate<IAssignmentService, CachingAssignmentService>(); 
            services.Decorate<ISubmissionService, CachingSubmissionService>(); 
            services.Decorate<IDepartmentService, CachingDepartmentService>();

            return services;
        }
    }
}
