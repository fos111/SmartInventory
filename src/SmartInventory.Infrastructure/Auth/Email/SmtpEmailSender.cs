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
}
