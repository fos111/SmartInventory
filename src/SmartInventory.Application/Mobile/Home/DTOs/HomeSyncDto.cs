using SmartInventory.Application.Mobile.Auth.DTOs;

namespace SmartInventory.Application.Mobile.Home.DTOs;

public class HomeSyncDto
{
    public MobileUserDto? User { get; set; }
    public HomeInventoryStatsDto? Stats { get; set; }
    public List<ActivityLogEntryDto> RecentActivity { get; set; } = new();
    public int UnreadNotifications { get; set; }
    public DateTime ServerTimestamp { get; set; }
}
