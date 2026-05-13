using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using QRCoder;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Notification.Enums;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Application.Asset.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _repository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IAssetHistoryService _historyService;
    private readonly INotificationService _notificationService;
    private readonly ILocationRepository _locationRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IMapper _mapper;

    public AssetService(
        IAssetRepository repository, 
        IBackgroundJobClient backgroundJobClient,
        IAssetHistoryService historyService,
        INotificationService notificationService,
        ILocationRepository locationRepository,
        IActivityLogService activityLogService,
        IMapper mapper)
    {
        _repository = repository;
        _backgroundJobClient = backgroundJobClient;
        _historyService = historyService;
        _notificationService = notificationService;
        _locationRepository = locationRepository;
        _activityLogService = activityLogService;
        _mapper = mapper;
    }

    private string GenerateAssetTag()
    {
        return $"AST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    public async Task<PagedResult<AssetDto>> GetAssetsAsync(AssetFilter filter)
    {
        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 100;

        var (items, totalCount) = await _repository.GetAssetsAsync(filter, page, pageSize);
        
        var dtos = new List<AssetDto>();
        foreach (var asset in items)
        {
            var dto = _mapper.Map<AssetDto>(asset);
            var zone = await _locationRepository.GetZoneByRoomCodeAsync(asset.CurrentRoomCode);
            if (zone != null)
            {
                dto.ZoneCode = zone.Code;
                dto.ZoneName = zone.Name;
            }
            dtos.Add(dto);
        }
        
        return new PagedResult<AssetDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) return null;
        var dto = _mapper.Map<AssetDto>(asset);
        var zone = await _locationRepository.GetZoneByRoomCodeAsync(asset.CurrentRoomCode);
        if (zone != null)
        {
            dto.ZoneCode = zone.Code;
            dto.ZoneName = zone.Name;
        }
        return dto;
    }

    public async Task<AssetDto?> GetAssetByTagAsync(string assetTag)
    {
        var asset = await _repository.GetByTagAsync(assetTag);
        if (asset == null) return null;
        var dto = _mapper.Map<AssetDto>(asset);
        var zone = await _locationRepository.GetZoneByRoomCodeAsync(asset.CurrentRoomCode);
        if (zone != null)
        {
            dto.ZoneCode = zone.Code;
            dto.ZoneName = zone.Name;
        }
        return dto;
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, Guid userId)
    {
        if (!string.IsNullOrEmpty(dto.AssetTag))
        {
            if (!await _repository.IsAssetTagUniqueAsync(dto.AssetTag))
                throw new InvalidOperationException($"Asset tag '{dto.AssetTag}' already exists.");
        }
        else
        {
            dto.AssetTag = GenerateAssetTag();
        }

        if (!await _repository.IsRoomCodeValidAsync(dto.CurrentRoomCode))
            throw new ArgumentException($"Room code '{dto.CurrentRoomCode}' is not valid.");

        var asset = _mapper.Map<AssetEntity>(dto);
        var created = await _repository.AddAsync(asset);

        await _activityLogService.TrackFacilityChangeAsync(
            "Created", "Asset", created.AssetTag, created.Name, null, userId);
        
        return _mapper.Map<AssetDto>(created);
    }

    public async Task<AssetDto> UpdateAssetAsync(Guid id, UpdateAssetDto dto, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (!await _repository.IsAssetTagUniqueAsync(dto.AssetTag, id))
            throw new InvalidOperationException($"Asset tag '{dto.AssetTag}' already exists.");

        if (!string.IsNullOrEmpty(dto.CurrentRoomCode) && !await _repository.IsRoomCodeValidAsync(dto.CurrentRoomCode))
            throw new ArgumentException($"Room code '{dto.CurrentRoomCode}' is not valid.");

        if (asset.Name != dto.Name)
            await _historyService.TrackChangeAsync(id, "Name", asset.Name, dto.Name, userId);
        if (asset.Description != dto.Description)
            await _historyService.TrackChangeAsync(id, "Description", asset.Description, dto.Description, userId);
        if (asset.Category != dto.Category)
            await _historyService.TrackChangeAsync(id, "Category", asset.Category, dto.Category, userId);
        if (asset.Status != dto.Status)
            await _historyService.TrackChangeAsync(id, "Status", asset.Status.ToString(), dto.Status.ToString(), userId);
        if (!string.IsNullOrEmpty(dto.CurrentRoomCode) && asset.CurrentRoomCode != dto.CurrentRoomCode)
            await _historyService.TrackChangeAsync(id, "CurrentRoomCode", asset.CurrentRoomCode, dto.CurrentRoomCode, userId);

        asset.Name = dto.Name;
        asset.Description = dto.Description;
        asset.Category = dto.Category;
        asset.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.CurrentRoomCode))
            asset.CurrentRoomCode = dto.CurrentRoomCode;

        var updated = await _repository.UpdateAsync(asset);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> MoveAssetAsync(Guid id, string newRoomCode, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (!await _repository.IsRoomCodeValidAsync(newRoomCode))
            throw new ArgumentException($"Room code '{newRoomCode}' is not valid.");

        var oldRoomCode = asset.CurrentRoomCode;
        await _historyService.TrackChangeAsync(id, "CurrentRoomCode", oldRoomCode, newRoomCode, userId);

        asset.CurrentRoomCode = newRoomCode;
        var updated = await _repository.UpdateAsync(asset);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> UpdateRfidAsync(Guid id, string rfidTagId, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        var normalizedRfid = NormalizeRfid(rfidTagId);
        
        if (!await _repository.IsRfidUniqueAsync(normalizedRfid, id))
            throw new InvalidOperationException($"RFID tag '{rfidTagId}' already assigned to another asset.");

        var oldRfid = asset.RfidTagId;
        await _historyService.TrackChangeAsync(id, "RfidTagId", oldRfid, normalizedRfid, userId);

        asset.RfidTagId = normalizedRfid;
        var updated = await _repository.UpdateAsync(asset);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> UpdateStatusAsync(Guid id, AssetStatus status, Guid userId, UserRole userRole = UserRole.Technicien)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (status == AssetStatus.Retired && userRole == UserRole.Technicien)
            throw new UnauthorizedAccessException("Only Supervisors can set asset status to Retired.");

        var oldStatus = asset.Status;
        await _historyService.TrackChangeAsync(id, "Status", oldStatus.ToString(), status.ToString(), userId);

        asset.Status = status;
        var updated = await _repository.UpdateAsync(asset);

        await CreateStatusChangeNotificationAsync(updated, status);

        return _mapper.Map<AssetDto>(updated);
    }

    private async Task CreateStatusChangeNotificationAsync(AssetEntity asset, AssetStatus newStatus)
    {
        var (type, eventType, title, message) = GetNotificationDetails(newStatus, asset.AssetTag);
        await _notificationService.CreateNotificationAsync(
            userId: Guid.Empty,
            assetId: asset.Id,
            type: type,
            title: title,
            message: message,
            eventType: eventType
        );
    }

    private (NotificationType Type, NotificationEventType EventType, string Title, string Message) GetNotificationDetails(AssetStatus newStatus, string assetTag)
    {
        return newStatus switch
        {
            AssetStatus.Active => (NotificationType.Info, NotificationEventType.EquipmentStatusOperational, "Asset Active", $"Asset {assetTag} is now Active"),
            AssetStatus.InStock => (NotificationType.Info, NotificationEventType.EquipmentStatusInStock, "Asset In Stock", $"Asset {assetTag} is now In Stock"),
            AssetStatus.Maintenance => (NotificationType.Warning, NotificationEventType.EquipmentStatusMaintenance, "Asset Maintenance", $"Asset {assetTag} scheduled for maintenance"),
            AssetStatus.CriticalIssue => (NotificationType.Critical, NotificationEventType.EquipmentStatusCriticalIssue, "Asset Critical", $"Asset {assetTag} has a critical issue"),
            AssetStatus.Lost => (NotificationType.Critical, NotificationEventType.EquipmentStatusLost, "Asset Lost", $"Asset {assetTag} marked as Lost"),
            AssetStatus.Retired => (NotificationType.Warning, NotificationEventType.EquipmentStatusRetired, "Asset Retired", $"Asset {assetTag} has been retired"),
            _ => (NotificationType.Info, NotificationEventType.EquipmentStatusOperational, "Status Changed", $"Asset {assetTag} status changed")
        };
    }

    public async Task<AssetDto> SetMaintenanceDueDateAsync(Guid id, DateTime? dueDate, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (dueDate.HasValue && dueDate.Value.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Maintenance due date must be today or future.");

        var oldDueDate = asset.MaintenanceDueDate?.ToString("yyyy-MM-dd");
        var newDueDate = dueDate?.ToString("yyyy-MM-dd") ?? "cleared";
        
        await _historyService.TrackChangeAsync(id, "MaintenanceDueDate", oldDueDate, newDueDate, userId);

        asset.MaintenanceDueDate = dueDate.HasValue
            ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc)
            : null;
        if (dueDate.HasValue)
            asset.LastMaintenanceDate = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(asset);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task DeleteAssetAsync(string id, Guid userId)
    {
        var asset = Guid.TryParse(id, out var guid)
            ? await _repository.GetByIdAsync(guid)
            : await _repository.GetByTagAsync(id);

        if (asset == null)
            throw new ArgumentException($"Asset with identifier '{id}' not found.");

        if (asset.Status != AssetStatus.Retired)
            throw new InvalidOperationException("Only retired assets can be permanently deleted.");

        await _historyService.TrackChangeAsync(asset.Id, "DeletedAt", null, DateTime.UtcNow.ToString(), userId);

        await _repository.DeleteAsync(asset.Id);
    }

    public async Task<byte[]> GenerateQrCodeAsync(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode($"ASSET:{asset.AssetTag}", QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);
        return qrBytes;
    }

    public async Task<IEnumerable<AssetReconciliationDto>> GetReconciliationAsync()
    {
        var discrepantAssets = await _repository.GetDiscrepantAssetsAsync();
        return _mapper.Map<List<AssetReconciliationDto>>(discrepantAssets);
    }

    public async Task<BulkImportResponse> ImportAssetsAsync(Stream csvStream, Guid userId)
    {
        // Eagerly read the stream before queueing to avoid disposal issues
        using var memoryStream = new MemoryStream();
        await csvStream.CopyToAsync(memoryStream);
        var csvBytes = memoryStream.ToArray();

        var jobId = _backgroundJobClient.Enqueue<IBulkImportJob>(
            job => job.RunAsync(Guid.NewGuid().ToString(), csvBytes, userId));

        return new BulkImportResponse
        {
            JobId = jobId,
            Status = "Queued"
        };
    }

    private string NormalizeRfid(string rfidTagId)
    {
        if (string.IsNullOrEmpty(rfidTagId))
            return string.Empty;
        
        return rfidTagId
            .Replace(" ", "")
            .Replace(":", "")
            .Replace("-", "")
            .ToUpperInvariant();
    }
}