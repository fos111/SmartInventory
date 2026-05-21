using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Notification.Enums;

namespace SmartInventory.Infrastructure.Asset.BackgroundJobs;

public class MaintenanceNotificationJob : IMaintenanceNotificationJob
{
    private readonly IAssetRepository _assetRepository;
    private readonly INotificationService _notificationService;

    public MaintenanceNotificationJob(
        IAssetRepository assetRepository,
        INotificationService notificationService)
    {
        _assetRepository = assetRepository;
        _notificationService = notificationService;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var dueSoonAssets = await _assetRepository.GetAssetsWithMaintenanceAsync(now, now.AddDays(7));

        if (dueSoonAssets.Count != 0)
        {
            var dueSoonDto = new CreateNotificationDto
            {
                EventType = NotificationEventType.MaintenanceDueSoon,
                Type = NotificationType.Info,
                Title = "Maintenance Due Soon",
                Message = $"{dueSoonAssets.Count} asset(s) have maintenance due within the next 7 days.",
                TargetRole = UserRole.Technicien
            };

            await _notificationService.CreateNotificationAsync(dueSoonDto, ct);
        }

        var overdueAssets = await _assetRepository.GetAssetsWithMaintenanceAsync(DateTime.MinValue, now);

        if (overdueAssets.Count != 0)
        {
            var overdueDto = new CreateNotificationDto
            {
                EventType = NotificationEventType.MaintenanceOverdue,
                Type = NotificationType.Warning,
                Title = "Maintenance Overdue",
                Message = $"{overdueAssets.Count} asset(s) have overdue maintenance.",
                TargetRole = UserRole.Supervisor
            };

            await _notificationService.CreateNotificationAsync(overdueDto, ct);
        }
    }
}
