namespace SmartInventory.Application.Auth.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
        Task SendVerificationEmailAsync(string toEmail, string verificationLink, CancellationToken ct = default);
    }
}