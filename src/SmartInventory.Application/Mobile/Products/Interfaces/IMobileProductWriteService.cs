using System;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Products.DTOs;

namespace SmartInventory.Application.Mobile.Products.Interfaces;

public interface IMobileProductWriteService
{
    Task<AssetScanDto?> CreateProductAsync(
        CreateProductRequestDto dto, Guid userId, CancellationToken ct = default);

    Task<AssetScanDto?> CreateProductFromMobileAsync(
        MobileProductCreateDto dto, Guid userId, CancellationToken ct = default);

    Task<AssetScanDto?> UpdateProductAsync(
        Guid id, MobileProductCreateDto dto, Guid userId, CancellationToken ct = default);

    Task<AssetScanDto?> UpdateProductStatusAsync(
        Guid id, string status, Guid userId, CancellationToken ct = default, string? note = null);

    Task<AssetScanDto?> UpdateProductBleIdAsync(
        Guid id, string? bleId, Guid userId, CancellationToken ct = default);

    Task<AssetScanDto?> UpdateProductPriceAsync(
        Guid id, string? price, Guid userId, CancellationToken ct = default);

    Task<(AssetScanDto? Data, string? Message)> MoveProductAsync(
        Guid id, string roomId, Guid userId, CancellationToken ct = default);

    Task<(bool Success, string? Message)> DeleteProductAsync(
        Guid id, Guid userId, CancellationToken ct = default);
}
