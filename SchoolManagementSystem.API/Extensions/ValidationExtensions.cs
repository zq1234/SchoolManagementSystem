using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Application.Validators;
using System.Reflection;

namespace SchoolManagementSystem.API.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            //services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            // Automatically register all validators in the entire Application assembly
            var assembly = Assembly.Load("SchoolManagementSystem.Application");
            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
