using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Asset.Entities;
using SmartInventory.Domain.Asset.Enums;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Application.Tests;

public class AssetServiceImportTests
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IBackgroundJobClient> _jobClientMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AssetService _service;

    public AssetServiceImportTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _jobClientMock = new Mock<IBackgroundJobClient>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _locationRepositoryMock = new Mock<ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _jobClientMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    [Fact]
    public async Task ImportAssetsAsync_ShouldEnqueueHangfireJob_WhenCalledWithCsvStream()
    {
        // Arrange
        var csvContent = "AST-001,Laptop Dell,Computer,LI1\n" +
                         "AST-002,Monitor HP,Display,LI2\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var userId = Guid.NewGuid();

        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("hangfire-job-id-123");

        // Act
        var result = await _service.ImportAssetsAsync(stream, userId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Queued");
        result.JobId.Should().NotBeNullOrEmpty();

        _jobClientMock.Verify(
            j => j.Create(It.Is<Job>(job => job.Type == typeof(IBulkImportJob)), It.IsAny<IState>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportAssetsAsync_ShouldEagerlyReadStream_ToAvoidDisposalBug()
    {
        // Arrange
        var csvContent = "AST-001,Laptop Dell,Computer,LI1\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var userId = Guid.NewGuid();

        _jobClientMock
            .Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<IState>()))
            .Returns("job-id");

        // Act
        await _service.ImportAssetsAsync(stream, userId);

        // Assert — the stream should be consumed (position at end) before the controller returns
        // This proves the stream data is eagerly read, not lazily captured
        stream.Position.Should().Be(csvBytes.Length);
    }
}
