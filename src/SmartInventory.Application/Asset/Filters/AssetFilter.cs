using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.Filters;

public class AssetFilter
{
    public string? RoomCode { get; set; }
    public string? Category { get; set; }
    public string? Group { get; set; }
    public AssetStatus? Status { get; set; }
    public string? Search { get; set; }
    public bool ShowDiscrepantOnly { get; set; }
    public DateTime? UpdatedAtFrom { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}