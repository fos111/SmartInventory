using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs.Reports;

public class StatusCountDto
{
    public AssetStatus Status { get; set; }
    public int Count { get; set; }
}
