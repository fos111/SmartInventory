using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Infrastructure.Data;
using SmartInventory.IoTLocation.Contracts;
using SmartInventory.IoTLocation.Interfaces;

namespace SmartInventory.IoTLocation.Services;

public class IoTLocationService : IIoTLocationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<IoTLocationService> _logger;

    public IoTLocationService(ApplicationDbContext dbContext, ILogger<IoTLocationService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
}
