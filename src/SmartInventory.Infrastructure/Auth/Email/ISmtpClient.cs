using System.Net.Mail;

namespace SmartInventory.Infrastructure.Auth.Email
{
    public interface ISmtpClient : IDisposable
    {
        Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default);
    }
}