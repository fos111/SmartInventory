using System.Net.Mail;
using Hangfire;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Auth.BackgroundJobs;
using SmartInventory.Infrastructure.Auth.Configuration;
using SmartInventory.Infrastructure.Auth.Email;

namespace SmartInventory.Infrastructure.Auth.BackgroundJobs;

[AutomaticRetry(Attempts = 3)]
public class EmailJob : IEmailJob
{
    private readonly ISmtpClient _smtpClient;
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailJob> _logger;

    public EmailJob(ISmtpClient smtpClient, SmtpSettings settings, ILogger<EmailJob> logger)
    {
        _smtpClient = smtpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await _smtpClient.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To}", to);
    }
}
