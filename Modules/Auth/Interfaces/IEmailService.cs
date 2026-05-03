namespace backend.Modules.Auth.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendWelcomeEmailAsync(string email, string displayName);
    Task SendPasswordResetEmailAsync(string email, string token);
}
