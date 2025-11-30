namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWelcomeEmailAsync(string to, string name);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
    }
}