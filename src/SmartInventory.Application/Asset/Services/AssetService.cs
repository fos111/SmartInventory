using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using BarcodeStandard;
using QRCoder;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Notification.DTOs;
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
    private readonly ICacheService? _cacheService;
    private readonly IBlobCacheService? _blobCacheService;

    public AssetService(
        IAssetRepository repository, 
        IBackgroundJobClient backgroundJobClient,
        IAssetHistoryService historyService,
        INotificationService notificationService,
        ILocationRepository locationRepository,
        IActivityLogService activityLogService,
        IMapper mapper,
        ICacheService? cacheService = null,
        IBlobCacheService? blobCacheService = null)
    {
        _repository = repository;
        _backgroundJobClient = backgroundJobClient;
        _historyService = historyService;
        _notificationService = notificationService;
        _locationRepository = locationRepository;
        _activityLogService = activityLogService;
        _mapper = mapper;
        _cacheService = cacheService;
        _blobCacheService = blobCacheService;
    }

    private async Task InvalidateCachesAsync(Guid? assetId = null)
    {
        if (_cacheService != null)
        {
            await _cacheService.RemoveByPrefixAsync("stats:");
            await _cacheService.RemoveByPrefixAsync("asset:");
        }

        if (_blobCacheService != null && assetId.HasValue)
        {
            await _blobCacheService.DeleteAsync($"qrcodes/asset-{assetId}.png");
            await _blobCacheService.DeleteAsync($"barcodes/asset-{assetId}.png");
        }
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

        await InvalidateCachesAsync(created.Id);

        await _activityLogService.TrackFacilityChangeAsync(
            "Created", "Asset", created.AssetTag, created.Name, null, userId);

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentCrudCreated,
            Type = NotificationType.Info,
            Title = "Asset Created",
            Message = $"Asset {created.AssetTag} ({created.Name}) has been created",
            AssetId = created.Id,
            TargetRole = UserRole.Supervisor
        });

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

        var changeParts = new List<string>();

        if (asset.Name != dto.Name)
        {
            await _historyService.TrackChangeAsync(id, "Name", asset.Name, dto.Name, userId);
            changeParts.Add($"renamed to \"{dto.Name}\"");
        }
        if (asset.Description != dto.Description)
        {
            await _historyService.TrackChangeAsync(id, "Description", asset.Description, dto.Description, userId);
        }
        if (asset.Category != dto.Category)
        {
            await _historyService.TrackChangeAsync(id, "Category", asset.Category, dto.Category, userId);
            changeParts.Add($"category → {dto.Category}");
        }
        if (asset.Status != dto.Status)
        {
            await _historyService.TrackChangeAsync(id, "Status", asset.Status.ToString(), dto.Status.ToString(), userId);
            changeParts.Add($"status → {dto.Status}");
        }
        if (!string.IsNullOrEmpty(dto.CurrentRoomCode) && asset.CurrentRoomCode != dto.CurrentRoomCode)
        {
            await _historyService.TrackChangeAsync(id, "CurrentRoomCode", asset.CurrentRoomCode, dto.CurrentRoomCode, userId);
            changeParts.Add($"moved to {dto.CurrentRoomCode}");
        }

        asset.Name = dto.Name;
        asset.Description = dto.Description;
        asset.Category = dto.Category;
        asset.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.CurrentRoomCode))
            asset.CurrentRoomCode = dto.CurrentRoomCode;

        var updated = await _repository.UpdateAsync(asset);
        await InvalidateCachesAsync(id);

        var message = changeParts.Count > 0
            ? $"{asset.Name} ({asset.AssetTag}) — {string.Join(", ", changeParts)}"
            : $"{asset.Name} ({asset.AssetTag}) was updated";

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentCrudUpdated,
            Type = NotificationType.Info,
            Title = "Asset Updated",
            Message = message,
            AssetId = updated.Id,
            TargetRole = UserRole.Supervisor
        });

        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> MoveAssetAsync(Guid id, string newRoomCode, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        // Accept both room UUID and room code
        // Flutter historically sends room UUIDs; resolve to room code if needed
        var roomCode = newRoomCode;
        if (Guid.TryParse(newRoomCode, out var roomGuid))
        {
            var room = await _locationRepository.GetRoomByIdAsync(roomGuid);
            if (room != null)
                roomCode = room.Code;
        }

        if (!await _repository.IsRoomCodeValidAsync(roomCode))
            throw new ArgumentException($"Room code '{roomCode}' is not valid.");

        var oldRoomCode = asset.CurrentRoomCode;
        await _historyService.TrackChangeAsync(id, "CurrentRoomCode", oldRoomCode, roomCode, userId);

        asset.CurrentRoomCode = roomCode;
        var updated = await _repository.UpdateAsync(asset);
        await InvalidateCachesAsync(id);
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
        await InvalidateCachesAsync(id);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> UpdateBleIdAsync(Guid id, string? bleId, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (!string.IsNullOrEmpty(bleId) && !await _repository.IsBleIdUniqueAsync(bleId, id))
            throw new InvalidOperationException($"BLE ID '{bleId}' already assigned to another asset. Clear it from the old asset first.");

        var oldValue = asset.BleId;
        await _historyService.TrackChangeAsync(id, "BleId", oldValue, bleId, userId);

        asset.BleId = bleId;
        var updated = await _repository.UpdateAsync(asset);
        await InvalidateCachesAsync(id);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> UpdatePriceAsync(Guid id, string? price, Guid userId)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        var oldValue = asset.Price;
        await _historyService.TrackChangeAsync(id, "Price", oldValue, price, userId);

        asset.Price = price;
        var updated = await _repository.UpdateAsync(asset);
        await InvalidateCachesAsync(id);
        return _mapper.Map<AssetDto>(updated);
    }

    public async Task<AssetDto> UpdateStatusAsync(Guid id, AssetStatus status, Guid userId, UserRole userRole = UserRole.Technicien, string? note = null)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        if (status == AssetStatus.Retired && userRole == UserRole.Technicien)
            throw new UnauthorizedAccessException("Only Supervisors can set asset status to Retired.");

        var oldStatus = asset.Status;

        // ── Same-Status Guard ────────────────────────────────────────────
        // Prevent side-effects (history entry, notification, cache invalidation)
        // when the status value hasn't actually changed.
        if (status == oldStatus)
        {
            // Updating the entry note while staying in Maintenance/CriticalIssue
            // is a legitimate operation — update note only, no status notification.
            if (!string.IsNullOrWhiteSpace(note) &&
                (status == AssetStatus.Maintenance || status == AssetStatus.CriticalIssue))
            {
                asset.StatusEntryNote = note.Trim();
                await _historyService.TrackChangeAsync(id, "StatusEntryNote", null, note.Trim(), userId);
                var updatedNote = await _repository.UpdateAsync(asset);
                await InvalidateCachesAsync(id);
                return _mapper.Map<AssetDto>(updatedNote);
            }

            // No meaningful change — return current state without side effects.
            return _mapper.Map<AssetDto>(asset);
        }

        // ── Status Entry Note Validation ─────────────────────────────────
        // Note is required when transitioning TO Maintenance or CriticalIssue
        var enteringMaintenance = status == AssetStatus.Maintenance;
        var enteringCritical = status == AssetStatus.CriticalIssue;

        if ((enteringMaintenance || enteringCritical) && string.IsNullOrWhiteSpace(note))
            throw new ArgumentException(
                $"A note describing the reason is required when changing status to {status}.");

        // ── Note Lifecycle Logic ─────────────────────────────────────────
        if (enteringMaintenance || enteringCritical)
        {
            // Entry note: describes what needs to be done / what is wrong
            // Preserve previous exit note (it's a resolution record)
            asset.StatusEntryNote = note?.Trim();
        }
        else
        {
            // Exiting Maintenance or CriticalIssue
            var exitingMaintenance = oldStatus == AssetStatus.Maintenance && status != AssetStatus.Maintenance;
            var exitingCritical = oldStatus == AssetStatus.CriticalIssue && status != AssetStatus.CriticalIssue;

            if (exitingMaintenance || exitingCritical)
            {
                // Exit note: describes what was done / how resolved (optional)
                if (!string.IsNullOrWhiteSpace(note))
                    asset.StatusExitNote = note.Trim();

                // Clear entry note since we're no longer in that state
                asset.StatusEntryNote = null;
            }
            else
            {
                // Transition between non-Maintenance/CriticalIssue statuses — no notes
                asset.StatusEntryNote = null;
            }
        }

        var oldStatusStr = oldStatus.ToString();
        await _historyService.TrackChangeAsync(id, "Status", oldStatusStr, status.ToString(), userId);

        // Track note changes in history
        if ((enteringMaintenance || enteringCritical) && !string.IsNullOrWhiteSpace(note))
        {
            await _historyService.TrackChangeAsync(id, "StatusEntryNote", null, note.Trim(), userId);
        }

        asset.Status = status;
        var updated = await _repository.UpdateAsync(asset);

        await InvalidateCachesAsync(id);
        await CreateStatusChangeNotificationAsync(updated, status, note);

        return _mapper.Map<AssetDto>(updated);
    }

    private async Task CreateStatusChangeNotificationAsync(AssetEntity asset, AssetStatus newStatus, string? note = null)
    {
        var (type, eventType, title, message, targetRole) = GetNotificationDetails(newStatus, asset.AssetTag, note);
        var dto = new CreateNotificationDto
        {
            EventType = eventType,
            Type = type,
            Title = title,
            Message = message,
            AssetId = asset.Id,
            TargetRole = targetRole
        };
        await _notificationService.CreateNotificationAsync(dto);
    }

    private (NotificationType Type, NotificationEventType EventType, string Title, string Message, UserRole TargetRole) GetNotificationDetails(AssetStatus newStatus, string assetTag, string? note = null)
    {
        var noteSuffix = !string.IsNullOrWhiteSpace(note) ? $": {note}" : string.Empty;
        return newStatus switch
        {
            AssetStatus.CriticalIssue => (NotificationType.Critical, NotificationEventType.EquipmentStatusCriticalIssue, "Asset Critical", $"Asset {assetTag} has a critical issue{noteSuffix}", UserRole.Supervisor),
            AssetStatus.Lost => (NotificationType.Critical, NotificationEventType.EquipmentStatusLost, "Asset Lost", $"Asset {assetTag} marked as Lost", UserRole.Supervisor),
            AssetStatus.Retired => (NotificationType.Warning, NotificationEventType.EquipmentStatusRetired, "Asset Retired", $"Asset {assetTag} has been retired", UserRole.Supervisor),
            AssetStatus.Maintenance => (NotificationType.Warning, NotificationEventType.EquipmentStatusMaintenance, "Asset Maintenance", $"Asset {assetTag} scheduled for maintenance{noteSuffix}", UserRole.Technicien),
            AssetStatus.Active => (NotificationType.Info, NotificationEventType.EquipmentStatusOperational, "Asset Active", $"Asset {assetTag} is now Active{noteSuffix}", UserRole.Technicien),
            AssetStatus.InStock => (NotificationType.Info, NotificationEventType.EquipmentStatusInStock, "Asset In Stock", $"Asset {assetTag} is now In Stock", UserRole.Technicien),
            _ => (NotificationType.Info, NotificationEventType.EquipmentStatusOperational, "Status Changed", $"Asset {assetTag} status changed", UserRole.Technicien)
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
        await InvalidateCachesAsync(id);

        if (dueDate.HasValue)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.MaintenanceScheduled,
                Type = NotificationType.Info,
                Title = "Maintenance Scheduled",
                Message = $"Maintenance scheduled for {updated.Name} ({updated.AssetTag}) on {dueDate.Value:yyyy-MM-dd}",
                AssetId = updated.Id,
                TargetRole = UserRole.Technicien
            });
        }

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
        await InvalidateCachesAsync(asset.Id);

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentCrudDeleted,
            Type = NotificationType.Warning,
            Title = "Asset Deleted",
            Message = $"Asset {asset.AssetTag} ({asset.Name}) has been permanently deleted",
            AssetId = asset.Id,
            TargetRole = UserRole.Supervisor
        });
    }

    public async Task<byte[]> GenerateQrCodeAsync(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        var cacheKey = $"qrcodes/asset-{id}.png";

        if (_blobCacheService != null)
        {
            var cached = await _blobCacheService.GetAsync(cacheKey);
            if (cached != null) return cached;
        }

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode($"ASSET:{asset.AssetTag}", QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        if (_blobCacheService != null)
            await _blobCacheService.SetAsync(cacheKey, qrBytes, "image/png");

        return qrBytes;
    }

    public async Task<byte[]> GenerateBarcodeAsync(Guid id, int width, int height)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {id} not found.");

        var cacheKey = $"barcodes/asset-{id}.png";

        if (_blobCacheService != null)
        {
            var cached = await _blobCacheService.GetAsync(cacheKey);
            if (cached != null) return cached;
        }

        using var barcode = new Barcode();
        barcode.IncludeLabel = true;
        barcode.Encode(BarcodeStandard.Type.Code128, asset.AssetTag, width, height);
        var barcodeBytes = barcode.GetImageData(SaveTypes.Png);

        if (_blobCacheService != null)
            await _blobCacheService.SetAsync(cacheKey, barcodeBytes, "image/png");

        return barcodeBytes;
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