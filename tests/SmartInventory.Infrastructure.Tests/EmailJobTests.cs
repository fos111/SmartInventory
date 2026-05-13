using System.Net.Mail;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Infrastructure.Auth.BackgroundJobs;
using SmartInventory.Infrastructure.Auth.Configuration;
using SmartInventory.Infrastructure.Auth.Email;
using Xunit;

namespace SmartInventory.Infrastructure.Tests;

public class EmailJobTests : InfrastructureTestBase
{
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly Mock<ILogger<EmailJob>> _loggerMock;
    private readonly SmtpSettings _settings;
    private readonly EmailJob _job;

    public EmailJobTests()
    {
        _smtpClientMock = new Mock<ISmtpClient>();
        _loggerMock = new Mock<ILogger<EmailJob>>();
        _settings = new SmtpSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "testuser",
            Password = "testpass",
            EnableSsl = true,
            FromEmail = "noreply@test.com",
            FromName = "TestApp"
        };
        _job = new EmailJob(_smtpClientMock.Object, _settings, _loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_SendsMailWithCorrectParameters()
    {
        _smtpClientMock
            .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _job.SendAsync("recipient@test.com", "Test Subject", "<p>Test body</p>");

        _smtpClientMock.Verify(
            x => x.SendMailAsync(
                It.Is<MailMessage>(m =>
                    m.To.ToString() == "recipient@test.com" &&
                    m.Subject == "Test Subject" &&
                    m.IsBodyHtml == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_SetsFromAddressFromSettings()
    {
        _smtpClientMock
            .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _job.SendAsync("recipient@test.com", "Subject", "Body");

        _smtpClientMock.Verify(
            x => x.SendMailAsync(
                It.Is<MailMessage>(m =>
                    m.From != null &&
                    m.From.Address == "noreply@test.com" &&
                    m.From.DisplayName == "TestApp"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_LogsInfoOnSuccess()
    {
        _smtpClientMock
            .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _job.SendAsync("recipient@test.com", "Subject", "Body");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("recipient@test.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ThrowsOnSmtpFailure()
    {
        _smtpClientMock
            .Setup(x => x.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmtpException("SMTP error"));

        var act = () => _job.SendAsync("recipient@test.com", "Test Subject", "<p>Test body</p>");

        await act.Should().ThrowAsync<SmtpException>();
    }
}
