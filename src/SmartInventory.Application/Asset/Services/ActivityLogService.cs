using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using ActivityLogEntity = SmartInventory.Domain.Asset.Entities.ActivityLog;

namespace SmartInventory.Application.Asset.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repository;

    public ActivityLogService(IActivityLogRepository repository)
    {
        _repository = repository;
    }

    public async Task TrackFacilityChangeAsync(
        string action,
        string entityType,
        string entityId,
        string entityName,
        string? details,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be empty", nameof(entityName));

        var log = new ActivityLogEntity
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            Details = details,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(log);
    }

    public async Task<IEnumerable<ActivityLogDto>> GetAllActivityLogsAsync(DateTime? from = null, DateTime? to = null)
    {
        var logs = await _repository.GetAllAsync(from, to);

        return logs.Select(l => new ActivityLogDto
        {
            Id = l.Id,
            Action = l.Action,
            AssetName = l.EntityName,
            AssetTag = l.EntityId,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Details = l.Details,
            ChangedBy = l.ChangedBy,
            ChangedAt = l.ChangedAt
        });
    }
}
