using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActivityLogEntity = SmartInventory.Domain.Asset.Entities.ActivityLog;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IActivityLogRepository
{
    Task<ActivityLogEntity> AddAsync(ActivityLogEntity log);
    Task<IEnumerable<ActivityLogEntity>> GetAllAsync(DateTime? from = null, DateTime? to = null);
}
