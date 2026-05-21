using System;
using System.IO;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IAssetService
{
    Task<PagedResult<AssetDto>> GetAssetsAsync(AssetFilter filter);
    Task<AssetDto?> GetAssetByIdAsync(Guid id);
    Task<AssetDto?> GetAssetByTagAsync(string assetTag);
    Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, Guid userId);
    Task<AssetDto> UpdateAssetAsync(Guid id, UpdateAssetDto dto, Guid userId);
    Task<AssetDto> MoveAssetAsync(Guid id, string newRoomCode, Guid userId);
    Task<AssetDto> UpdateRfidAsync(Guid id, string rfidTagId, Guid userId);
    Task<AssetDto> UpdateBleIdAsync(Guid id, string? bleId, Guid userId);
    Task<AssetDto> UpdatePriceAsync(Guid id, string? price, Guid userId);
    Task<AssetDto> UpdateStatusAsync(Guid id, AssetStatus status, Guid userId, UserRole userRole = UserRole.Technicien, string? note = null);
    Task<AssetDto> SetMaintenanceDueDateAsync(Guid id, DateTime? dueDate, Guid userId);
    Task DeleteAssetAsync(string id, Guid userId);
    Task<byte[]> GenerateQrCodeAsync(Guid id);
    Task<byte[]> GenerateBarcodeAsync(Guid id, int width, int height);
    Task<IEnumerable<AssetReconciliationDto>> GetReconciliationAsync();
    Task<BulkImportResponse> ImportAssetsAsync(Stream csvStream, Guid userId);
}