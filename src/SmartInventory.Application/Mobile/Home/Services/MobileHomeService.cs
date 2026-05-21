using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.Mobile.Home.DTOs;
using SmartInventory.Application.Mobile.Home.Interfaces;
using SmartInventory.Application.Notification.Interfaces;

namespace SmartInventory.Application.Mobile.Home.Services;

public class MobileHomeService : IMobileHomeService
{
    private readonly IMobileAuthService _authService;
    private readonly IReportingService _reportingService;
    private readonly IActivityLogService _activityLogService;
    private readonly INotificationService _notificationService;

    public MobileHomeService(
        IMobileAuthService authService,
        IReportingService reportingService,
        IActivityLogService activityLogService,
        INotificationService notificationService)
    {
        _authService = authService;
        _reportingService = reportingService;
        _activityLogService = activityLogService;
        _notificationService = notificationService;
    }

    public async Task<HomeSyncDto> GetHomeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _authService.GetProfileAsync(userId, ct);
        var stats = await _reportingService.GetStatusSummaryAsync();
        var allActivity = await _activityLogService.GetAllActivityLogsAsync(DateTime.UtcNow.AddDays(-7), null);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        var homeStats = new HomeInventoryStatsDto
        {
            InStock = stats.FirstOrDefault(s => s.Status == "InStock")?.Count ?? 0,
            UnderMaintenance = stats.FirstOrDefault(s => s.Status == "Maintenance")?.Count ?? 0,
            Critical = stats.FirstOrDefault(s => s.Status == "Critical")?.Count ?? 0,
            Retired = stats.FirstOrDefault(s => s.Status == "Retired")?.Count ?? 0
        };

        var recentActivity = allActivity
            .OrderByDescending(a => a.ChangedAt)
            .Take(10)
            .Select(a => new ActivityLogEntryDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityName = a.AssetName,
                Details = a.Details,
                ChangedAt = a.ChangedAt,
                ChangedByName = a.ChangedByName
            })
            .ToList();

        return new HomeSyncDto
        {
            User = user,
            Stats = homeStats,
            RecentActivity = recentActivity,
            UnreadNotifications = unreadCount,
            ServerTimestamp = DateTime.UtcNow
        };
    }
}
