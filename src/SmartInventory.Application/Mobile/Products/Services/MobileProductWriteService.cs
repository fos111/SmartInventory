using System;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Mobile.Products.Services;

public class MobileProductWriteService : IMobileProductWriteService
{
    private readonly IAssetService _assetService;
    private readonly ILocationRepository _locationRepository;

    public MobileProductWriteService(
        IAssetService assetService,
        ILocationRepository locationRepository)
    {
        _assetService = assetService;
        _locationRepository = locationRepository;
    }

    public async Task<AssetScanDto?> CreateProductAsync(
        CreateProductRequestDto dto, Guid userId, CancellationToken ct = default)
    {
        var createDto = MapToCreateAssetDto(dto);
        var asset = await _assetService.CreateAssetAsync(createDto, userId);
        return MapToScanDto(asset);
    }

    public async Task<AssetScanDto?> CreateProductFromMobileAsync(
        MobileProductCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var roomCode = await ResolveRoomCodeAsync(dto.RoomId);
        var status = ParseStatus(null);

        var createDto = new CreateAssetDto
        {
            AssetTag = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Type ?? "General",
            Status = status,
            CurrentRoomCode = roomCode ?? string.Empty,
            PhotoPath = dto.PhotoPath,
            Price = dto.Price,
            BleId = dto.BleId
        };

        try
        {
            var asset = await _assetService.CreateAssetAsync(createDto, userId);
            return MapToScanDto(asset);
        }
        catch (ArgumentException ex)
        {
            return null;
        }
    }

    public async Task<AssetScanDto?> UpdateProductAsync(
        Guid id, MobileProductCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var roomCode = await ResolveRoomCodeAsync(dto.RoomId);

        var updateDto = new UpdateAssetDto
        {
            AssetTag = dto.Sku ?? string.Empty,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Type ?? "General",
            Status = AssetStatus.Active, // default for mobile updates
            CurrentRoomCode = roomCode ?? string.Empty,
            PhotoPath = dto.PhotoPath,
            Price = dto.Price
        };

        try
        {
            var asset = await _assetService.UpdateAssetAsync(id, updateDto, userId);
            return MapToScanDto(asset);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public async Task<AssetScanDto?> UpdateProductStatusAsync(
        Guid id, string status, Guid userId, CancellationToken ct = default, string? note = null)
    {
        if (!Enum.TryParse<AssetStatus>(status, ignoreCase: true, out var parsed))
            return null;

        try
        {
            var asset = await _assetService.UpdateStatusAsync(id, parsed, userId, note: note);
            return MapToScanDto(asset);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public async Task<AssetScanDto?> UpdateProductBleIdAsync(
        Guid id, string? bleId, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var asset = await _assetService.UpdateBleIdAsync(id, bleId, userId);
            return MapToScanDto(asset);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task<AssetScanDto?> UpdateProductPriceAsync(
        Guid id, string? price, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var asset = await _assetService.UpdatePriceAsync(id, price, userId);
            return MapToScanDto(asset);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public async Task<(AssetScanDto? Data, string? Message)> MoveProductAsync(
        Guid id, string roomId, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var asset = await _assetService.MoveAssetAsync(id, roomId, userId);
            return (MapToScanDto(asset), null);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Message)> DeleteProductAsync(
        Guid id, Guid userId, CancellationToken ct = default)
    {
        try
        {
            await _assetService.DeleteAssetAsync(id.ToString(), userId);
            return (true, null);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        catch (ArgumentException)
        {
            return (false, $"Product with ID '{id}' not found.");
        }
    }

    private async Task<string?> ResolveRoomCodeAsync(string? roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            return null;

        // Try parsing as UUID (room ID from Flutter)
        if (Guid.TryParse(roomId, out var roomGuid))
        {
            var room = await _locationRepository.GetRoomByIdAsync(roomGuid);
            return room?.Code;
        }

        // Already a room code (e.g., "LI1")
        return roomId;
    }

    private static AssetStatus ParseStatus(string? status)
    {
        if (status != null && Enum.TryParse<AssetStatus>(status, ignoreCase: true, out var parsed))
            return parsed;
        return AssetStatus.InStock;
    }

    private static CreateAssetDto MapToCreateAssetDto(CreateProductRequestDto dto)
    {
        var status = dto.Status != null
            && Enum.TryParse<AssetStatus>(dto.Status, ignoreCase: true, out var parsed)
            ? parsed
            : AssetStatus.InStock;

        return new CreateAssetDto
        {
            AssetTag = dto.AssetTag,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Status = status,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            CurrentRoomCode = dto.CurrentRoomCode,
            InstallDate = dto.InstallDate,
            LastServiceDate = dto.LastServiceDate
        };
    }

    private static AssetScanDto MapToScanDto(AssetDto asset)
    {
        return new AssetScanDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Status = asset.Status.ToString(),
            CurrentRoomCode = asset.CurrentRoomCode,
            PhotoUrl = asset.PhotoPath,
            StatusEntryNote = asset.StatusEntryNote,
            StatusExitNote = asset.StatusExitNote
        };
    }
}
