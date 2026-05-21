namespace SmartInventory.Application.Mobile.Home.DTOs;

public class HomeInventoryStatsDto
{
    public int InStock { get; set; }
    public int UnderMaintenance { get; set; }
    public int Critical { get; set; }
    public int Retired { get; set; }
}
