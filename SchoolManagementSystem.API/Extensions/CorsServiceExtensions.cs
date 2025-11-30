using Microsoft.Extensions.DependencyInjection;

namespace SchoolManagementSystem.API.Extensions
{
    public static class CorsServiceExtensions
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowAnyOrigin();
                });
            });

            return services;
        }
    }
}
