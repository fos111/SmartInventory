using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdateAssetStatusDto
{
    public AssetStatus Status { get; set; }
}