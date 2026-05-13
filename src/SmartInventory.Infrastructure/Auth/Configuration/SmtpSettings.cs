namespace SmartInventory.Infrastructure.Auth.Configuration
{
    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = "live.smtp.mailtrap.io";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = "noreply@smartinventory.local";
        public string FromName { get; set; } = "SmartInventory";
    }
}