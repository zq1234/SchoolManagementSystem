using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("""
                Sending Email:
                To: {To}
                Subject: {Subject}
                Body: {Body}
                """, to, subject, body);

            await Task.Delay(100);
        }

        public async Task SendWelcomeEmailAsync(string to, string name)
        {
            var subject = "Welcome to School Management System";
            var body = $"""
                Dear {name},

                Welcome to School Management System! Your account has been successfully created.

                You can now login to the system using your credentials.

                Best regards,
                School Management System Team
                """;

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $"""
                You have requested to reset your password.

                Please use the following token to reset your password: {resetToken}

                If you did not request this, please ignore this email.

                Best regards,
                School Management System Team
                """;

            await SendEmailAsync(to, subject, body);
        }
    }
}