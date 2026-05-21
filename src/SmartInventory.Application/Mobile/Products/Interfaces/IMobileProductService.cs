using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Products.DTOs;

namespace SmartInventory.Application.Mobile.Products.Interfaces;

public interface IMobileProductService
{
    Task<MobilePagedResultDto<AssetListItemDto>> GetProductsAsync(
        MobileProductFilterDto filter, CancellationToken ct = default);

    Task<AssetScanDto?> GetProductByIdAsync(
        Guid id, CancellationToken ct = default);

    Task<AssetScanDto?> ScanByTagAsync(
        string assetTag, Guid userId, CancellationToken ct = default);

    Task<IEnumerable<ScanHistoryEntryDto>> GetScanHistoryAsync(
        DateTime? from, DateTime? to, Guid userId, CancellationToken ct = default);
}
