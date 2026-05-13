using Hangfire;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Auth.BackgroundJobs;
using SmartInventory.Application.Auth.Interfaces;

namespace SmartInventory.Infrastructure.Auth.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IBackgroundJobClient backgroundJobClient, ILogger<SmtpEmailService> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            _backgroundJobClient.Enqueue<IEmailJob>(job => job.SendAsync(to, subject, htmlBody));
            _logger.LogDebug("Enqueued email to {To}: {Subject}", to, subject);
            return Task.CompletedTask;
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
}
