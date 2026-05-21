using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Auth.BackgroundJobs;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartInventory.Infrastructure.Auth.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IBackgroundJobClient backgroundJobClient,
        ApplicationDbContext context,
        ILogger<SmtpEmailSender> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _context = context;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _backgroundJobClient.Enqueue<IEmailJob>(job => job.SendAsync(to, subject, htmlBody));
        _logger.LogDebug("Enqueued email to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public async Task SendToRoleAsync(UserRole role, string subject, string htmlBody, CancellationToken ct = default)
    {
        var recipients = await _context.Users
            .Where(u => u.Role == role)
            .Select(u => u.Email)
            .ToListAsync(ct);

        foreach (var email in recipients)
        {
            _backgroundJobClient.Enqueue<IEmailJob>(job => job.SendAsync(email, subject, htmlBody));
        }

        _logger.LogDebug("Enqueued {Count} emails for role {Role}", recipients.Count, role);
    }

    public Task SendVerificationEmailAsync(string toEmail, string verificationLink, CancellationToken ct = default)
    {
        var htmlBody = BuildVerificationTemplate(verificationLink);
        return SendEmailAsync(toEmail, "Verify your email - SmartInventory", htmlBody, ct);
    }

    private static string BuildVerificationTemplate(string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; background: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Verify your email address</h2>
        <p>Thank you for registering with SmartInventory. Please verify your email by clicking the button below:</p>
        <a href='{verificationLink}' class='button'>Verify Email</a>
        <p>If the button doesn't work, copy and paste this link into your browser:</p>
        <p>{verificationLink}</p>
        <p>This link expires in 24 hours.</p>
        <div class='footer'>
            <p>If you didn't create an account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";
    }
}
