using System.Text.Json;
//using FluentValidation;
using SchoolManagementSystem.Core.Exceptions;

namespace SchoolManagementSystem.API.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            object response;
            int statusCode;

            switch (exception)
            {
                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    response = new
                    {
                        error = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case UnauthorizedException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    response = new
                    {
                        error = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case ForbiddenException:
                    statusCode = StatusCodes.Status403Forbidden;
                    response = new
                    {
                        error = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case BadRequestException:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = new
                    {
                        error = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case ValidationException ve:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = new
                    {
                        error = "Validation failed",
                        details = ve.Errors,  // FIXED → This is already Dictionary<string, string[]>
                        timestamp = DateTime.UtcNow
                    };
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    response = new
                    {
                        error = "An internal server error occurred",
                        details = _env.IsDevelopment() ? exception.StackTrace : null,
                        timestamp = DateTime.UtcNow
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}