namespace SmartInventory.Application.Mobile.Lookup.DTOs;

public class MobileInventoryStatsDto
{
    public int InStock { get; set; }
    public int Maintenance { get; set; }
    public int Critical { get; set; }
    public int Lost { get; set; }
    public int Retired { get; set; }
}
