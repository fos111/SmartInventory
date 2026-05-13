using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendToRoleAsync(UserRole role, string subject, string htmlBody, CancellationToken ct = default);
}