using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs.Reports;

/// <summary>
/// Per-category per-status count from SQL GROUP BY.
/// Used by CategoryStocktake to avoid loading all assets into memory.
/// </summary>
public class CategoryStatusBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public int Count { get; set; }
}
