namespace SmartInventory.Application.Mobile.Products.DTOs;

public class MobileProductFilterDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? RoomCode { get; set; }
    public string? Department { get; set; }
    public string? Type { get; set; }
    public DateTime? Since { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
