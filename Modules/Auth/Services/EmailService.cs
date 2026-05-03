using System.Net;
using System.Net.Mail;
using backend.Modules.Auth.Interfaces;

namespace backend.Modules.Auth.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly string _templatesPath;

    public EmailService(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _templatesPath = Path.Combine(env.ContentRootPath, "Modules", "Auth", "Templates");
    }

    public async Task SendVerificationEmailAsync(string email, string code)
    {
        var template = await File.ReadAllTextAsync(Path.Combine(_templatesPath, "Verify-Email", "Index.html"));
        var body = template.Replace("{{CODE}}", code);

        await SendEmailAsync(email, "Verify Your Mosaic Account", body);
    }

    public async Task SendWelcomeEmailAsync(string email, string displayName)
    {
        var template = await File.ReadAllTextAsync(Path.Combine(_templatesPath, "New-User-Sign-In", "Index.html"));
        var body = template.Replace("{{NAME}}", displayName);

        await SendEmailAsync(email, "Welcome to Mosaic!", body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string code)
    {
        var template = await File.ReadAllTextAsync(Path.Combine(_templatesPath, "Reset-Password", "Index.html"));
        var body = template.Replace("{{CODE}}", code);

        await SendEmailAsync(email, "Mosaic - Password Reset Code", body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var host = _config["SMTP_HOST"];
        var port = int.Parse(_config["SMTP_PORT"] ?? "587");
        var email = _config["SMTP_EMAIL"];
        var password = _config["SMTP_PASSWORD"];
        var fromName = _config["SMTP_FROM_NAME"] ?? "Mosaic";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(email!, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
