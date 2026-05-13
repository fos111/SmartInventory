using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Application.Auth.BackgroundJobs;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Infrastructure.Auth.Email;
using Xunit;

namespace SmartInventory.Infrastructure.Tests;

public class SmtpEmailServiceTests : InfrastructureTestBase
{
    private readonly Mock<IBackgroundJobClient> _jobClientMock;
    private readonly Mock<ILogger<SmtpEmailService>> _loggerMock;
    private readonly SmtpEmailService _service;

    public SmtpEmailServiceTests()
    {
        _jobClientMock = new Mock<IBackgroundJobClient>();
        _loggerMock = new Mock<ILogger<SmtpEmailService>>();
        _service = new SmtpEmailService(_jobClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_EnqueuesEmailJob()
    {
        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-id");

        await _service.SendEmailAsync("recipient@test.com", "Test Subject", "<p>Test body</p>");

        _jobClientMock.Verify(
            j => j.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IEmailJob) &&
                    job.Method.Name == nameof(IEmailJob.SendAsync)),
                It.IsAny<IState>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_EnqueuesWithCorrectParameters()
    {
        string? capturedTo = null;
        string? capturedSubject = null;
        string? capturedBody = null;

        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-id")
            .Callback<Job, IState>((job, state) =>
            {
                capturedTo = job.Args[0] as string;
                capturedSubject = job.Args[1] as string;
                capturedBody = job.Args[2] as string;
            });

        await _service.SendEmailAsync("recipient@test.com", "Test Subject", "<p>Test body</p>");

        capturedTo.Should().Be("recipient@test.com");
        capturedSubject.Should().Be("Test Subject");
        capturedBody.Should().Be("<p>Test body</p>");
    }

    [Fact]
    public async Task SendVerificationEmailAsync_EnqueuesWithVerificationSubject()
    {
        string? capturedSubject = null;
        string? capturedBody = null;

        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-id")
            .Callback<Job, IState>((job, state) =>
            {
                capturedSubject = job.Args[1] as string;
                capturedBody = job.Args[2] as string;
            });

        await _service.SendVerificationEmailAsync("user@test.com", "https://app.com/verify?token=abc");

        capturedSubject.Should().Be("Verify your email - SmartInventory");
        capturedBody.Should().Contain("https://app.com/verify?token=abc");
        capturedBody.Should().Contain("Verify your email address");
        capturedBody.Should().Contain("24 hours");
    }

    [Fact]
    public async Task SendVerificationEmailAsync_EnqueuesEmailJob()
    {
        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-id");

        await _service.SendVerificationEmailAsync("user@test.com", "https://app.com/verify");

        _jobClientMock.Verify(
            j => j.Create(
                It.Is<Job>(job => job.Type == typeof(IEmailJob)),
                It.IsAny<IState>()),
            Times.Once);
    }
}
