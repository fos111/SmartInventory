using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QRCoder;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Reports.DTOs;
using SmartInventory.Application.Mobile.Reports.Interfaces;

namespace SmartInventory.Application.Mobile.Reports.Services;

public class MobileReportService : IMobileReportService
{
    private readonly IReportingService _reportingService;
    private readonly ILocationService _locationService;
    private readonly ILocationRepository _locationRepo;
    private readonly IPdfReportService _pdfReportService;
    private readonly IBlobCacheService? _blobCacheService;

    public MobileReportService(
        IReportingService reportingService,
        ILocationService locationService,
        ILocationRepository locationRepo,
        IPdfReportService pdfReportService,
        IBlobCacheService? blobCacheService = null)
    {
        _reportingService = reportingService;
        _locationService = locationService;
        _locationRepo = locationRepo;
        _pdfReportService = pdfReportService;
        _blobCacheService = blobCacheService;
    }

    public async Task<byte[]?> GetRoomFicheAsync(string roomCode)
    {
        var audit = await _reportingService.GetRoomAuditAsync(roomCode);
        if (audit == null)
            return null;

        return await _pdfReportService.GenerateRoomAuditAsync(audit);
    }

    public async Task<byte[]?> GetRoomJournalAsync(string roomCode, DateTime? from, DateTime? to)
    {
        var room = await _locationService.GetRoomByCodeAsync(roomCode);
        if (room == null)
            return null;

        var zone = await _locationRepo.GetZoneByRoomCodeAsync(roomCode);

        var activityEntries = await _reportingService.GetActivityLogAsync(from, to, null);

        var filteredEntries = activityEntries
            .Where(e => e.OldValue == roomCode || e.NewValue == roomCode)
            .Select(e => new RoomJournalEntryDto
            {
                Id = e.Id,
                AssetTag = e.AssetTag,
                AssetName = e.AssetName,
                Action = e.Action,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                ChangedByName = e.ChangedByName,
                ChangedAt = e.ChangedAt
            })
            .ToList();

        var journal = new RoomJournalDto
        {
            RoomCode = roomCode,
            RoomName = room.Name,
            DepartmentName = zone?.Name ?? string.Empty,
            GeneratedAt = DateTime.UtcNow,
            FromDate = from,
            ToDate = to,
            Entries = filteredEntries
        };

        return await _pdfReportService.GenerateRoomJournalAsync(journal);
    }

    public async Task<byte[]?> GetDepartmentQrAsync(Guid deptId)
    {
        var zone = await _locationRepo.GetZoneByIdAsync(deptId);
        if (zone == null)
            return null;

        var cacheKey = $"qrcodes/department-{deptId}.png";
        return await GetOrGenerateQrAsync(cacheKey, $"DEPT:{zone.Code}");
    }

    public async Task<byte[]> GetDepartmentQrByCodeAsync(string code)
    {
        var cacheKey = $"qrcodes/department-code-{code}.png";
        return await GetOrGenerateQrAsync(cacheKey, $"DEPT:{code}") ?? [];
    }

    public async Task<byte[]?> GetRoomQrAsync(string roomCode)
    {
        var room = await _locationService.GetRoomByCodeAsync(roomCode);
        if (room == null)
            return null;

        var cacheKey = $"qrcodes/room-{roomCode}.png";
        return await GetOrGenerateQrAsync(cacheKey, $"ROOM:{roomCode}");
    }

    public async Task<byte[]> GetIsetQrAsync()
    {
        var hierarchy = await _locationService.GetHierarchyAsync();
        var zoneCodes = hierarchy.Site.Zones
            .Select(z => z.Code)
            .ToList();

        var data = zoneCodes.Count > 0
            ? $"ISET:{string.Join(",", zoneCodes)}"
            : "ISET:";

        return await GetOrGenerateQrAsync("qrcodes/iset.png", data) ?? [];
    }

    private async Task<byte[]?> GetOrGenerateQrAsync(string cacheKey, string qrData)
    {
        if (_blobCacheService != null)
        {
            var cached = await _blobCacheService.GetAsync(cacheKey);
            if (cached != null) return cached;
        }

        var bytes = GenerateQrCode(qrData);

        if (_blobCacheService != null)
            await _blobCacheService.SetAsync(cacheKey, bytes, "image/png");

        return bytes;
    }

    public async Task InvalidateIsetQrAsync()
    {
        if (_blobCacheService != null)
            await _blobCacheService.DeleteAsync("qrcodes/iset.png");
    }

    private static byte[] GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
