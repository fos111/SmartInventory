using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Caching;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Domain.Asset.Entities;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Infrastructure.Data;
using SmartInventory.IoTLocation.Contracts;
using SmartInventory.IoTLocation.Interfaces;

namespace SmartInventory.IoTLocation.Services;

public class IoTLocationService : IIoTLocationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<IoTLocationService> _logger;
    private readonly ICacheService? _cacheService;

    public IoTLocationService(
        ApplicationDbContext dbContext,
        INotificationService notificationService,
        ILogger<IoTLocationService> logger,
        ICacheService? cacheService = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService;
    }

    public async Task<LocationProcessingResult> ProcessLocationAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Deserialize<IoTLocationMessage>(jsonPayload);
            if (message == null)
            {
                _logger.LogError("Failed to deserialize MQTT payload: {Payload}", jsonPayload);
                return new LocationProcessingResult { Success = false, ErrorMessage = "Invalid JSON payload" };
            }

            if (message.AssetId == Guid.Empty)
            {
                _logger.LogWarning("Invalid assetId in payload: {Payload}", jsonPayload);
                return new LocationProcessingResult { Success = false, ErrorMessage = "Invalid assetId" };
            }

            if (string.IsNullOrWhiteSpace(message.RoomCode))
            {
                _logger.LogWarning("Missing roomCode in payload: {Payload}", jsonPayload);
                return new LocationProcessingResult { Success = false, ErrorMessage = "Missing roomCode" };
            }

            // Pre-DB rate limit: skip if this asset was processed within the last 30 seconds
            if (_cacheService != null)
            {
                var rateKey = $"iot:ratelimit:asset:{message.AssetId}";
                var rateLimited = await _cacheService.GetAsync<string>(rateKey);
                if (rateLimited != null)
                {
                    _logger.LogInformation("Rate limited asset {AssetId}, skipping", message.AssetId);
                    return new LocationProcessingResult { Success = true, AssetId = message.AssetId.ToString(), RoomCode = message.RoomCode };
                }
                await _cacheService.SetAsync(rateKey, "1", TimeSpan.FromSeconds(30));

                // Exact message dedup (QoS-1 redelivery)
                var seenKey = $"iot:seen:{message.AssetId}:{message.Timestamp.Ticks}";
                var seen = await _cacheService.GetAsync<string>(seenKey);
                if (seen != null)
                {
                    _logger.LogInformation("Duplicate message for asset {AssetId}, skipping", message.AssetId);
                    return new LocationProcessingResult { Success = true, AssetId = message.AssetId.ToString(), RoomCode = message.RoomCode };
                }
                await _cacheService.SetAsync(seenKey, "1", TimeSpan.FromDays(1));
            }

            var asset = await _dbContext.Assets.FindAsync([message.AssetId], cancellationToken);
            if (asset == null)
            {
                _logger.LogWarning("Asset not found: {AssetId}", message.AssetId);
                return new LocationProcessingResult { Success = false, ErrorMessage = $"Asset {message.AssetId} not found" };
            }

            var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Code == message.RoomCode, cancellationToken);
            if (!roomExists)
            {
                _logger.LogWarning("Room code not found: {RoomCode}", message.RoomCode);
                return new LocationProcessingResult { Success = false, ErrorMessage = $"Room {message.RoomCode} not found" };
            }

            if (asset.DetectedRoomCode == message.RoomCode)
            {
                _logger.LogInformation("Asset {AssetId} already in room {RoomCode}, skipping update", message.AssetId, message.RoomCode);
                return new LocationProcessingResult { Success = true, AssetId = message.AssetId.ToString(), RoomCode = message.RoomCode };
            }

            var lastUpdate = asset.LastDetectedUpdate;
            if (lastUpdate.HasValue && message.Timestamp < lastUpdate.Value)
            {
                _logger.LogInformation("Stale message for asset {AssetId}, payload timestamp {Timestamp} older than last update {LastUpdate}", 
                    message.AssetId, message.Timestamp, lastUpdate.Value);
                return new LocationProcessingResult { Success = true, AssetId = message.AssetId.ToString(), RoomCode = message.RoomCode };
            }

            var history = new AssetLocationHistory
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                PreviousRoomCode = asset.DetectedRoomCode,
                NewRoomCode = message.RoomCode,
                Source = "IoT",
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.AssetLocationHistories.Add(history);

            var previousRoom = asset.DetectedRoomCode;
            asset.DetectedRoomCode = message.RoomCode;
            asset.LastDetectedUpdate = message.Timestamp;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset {AssetId} location updated: {PreviousRoom} → {NewRoom}", 
                message.AssetId, previousRoom, message.RoomCode);

            if (asset.CurrentRoomCode != asset.DetectedRoomCode)
            {
                await NotifyLocationMismatchAsync(asset, cancellationToken);
            }

            return new LocationProcessingResult { Success = true, AssetId = message.AssetId.ToString(), RoomCode = message.RoomCode };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize MQTT payload: {Payload}", jsonPayload);
            return new LocationProcessingResult { Success = false, ErrorMessage = "Invalid JSON format" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing location message: {Payload}", jsonPayload);
            return new LocationProcessingResult { Success = false, ErrorMessage = "Internal processing error" };
        }
    }

    private async Task NotifyLocationMismatchAsync(Asset asset, CancellationToken ct)
    {
        try
        {
            var dto = new CreateNotificationDto
            {
                EventType = NotificationEventType.LocationMismatch,
                Type = NotificationType.Warning,
                Title = "Location Mismatch Detected",
                Message = $"Asset '{asset.Name}' ({asset.AssetTag}) was detected in room '{asset.DetectedRoomCode}' but is recorded in room '{asset.CurrentRoomCode}'.",
                AssetId = asset.Id,
                TargetRole = SmartInventory.Domain.Auth.Enums.UserRole.Supervisor
            };

            await _notificationService.CreateNotificationAsync(dto, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send location mismatch notification for asset {AssetId}", asset.Id);
        }
    }
}
