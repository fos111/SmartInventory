namespace SmartInventory.Application.Auth.BackgroundJobs;

public interface IEmailJob
{
    Task SendAsync(string to, string subject, string htmlBody);
}
