using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Application.Behaviors;

namespace SchoolManagementSystem.API.Extensions
{
    public static class MediatorExtensions
    {
        public static IServiceCollection AddMediatorServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            return services;
        }
    }
}
