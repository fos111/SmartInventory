using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IActivityLogService
{
    Task TrackFacilityChangeAsync(string action, string entityType, string entityId, string entityName, string? details, Guid userId);
    Task<IEnumerable<ActivityLogDto>> GetAllActivityLogsAsync(DateTime? from = null, DateTime? to = null);
}
