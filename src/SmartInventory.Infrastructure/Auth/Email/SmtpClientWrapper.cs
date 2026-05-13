using System.Net.Mail;

namespace SmartInventory.Infrastructure.Auth.Email
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _smtpClient;

        public SmtpClientWrapper(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default)
        {
            return _smtpClient.SendMailAsync(message, cancellationToken);
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }
    }
}