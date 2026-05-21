using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCoder;
using SmartInventory.Application.Asset.Interfaces;
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

    public MobileReportService(
        IReportingService reportingService,
        ILocationService locationService,
        ILocationRepository locationRepo,
        IPdfReportService pdfReportService)
    {
        _reportingService = reportingService;
        _locationService = locationService;
        _locationRepo = locationRepo;
        _pdfReportService = pdfReportService;
    }

    public async Task<byte[]?> GetRoomFicheAsync(string roomCode)
    {
        var audit = await _reportingService.GetRoomAuditAsync(roomCode);
        if (audit == null)
            return null;

        return _pdfReportService.GenerateRoomAudit(audit);
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

        return _pdfReportService.GenerateRoomJournal(journal);
    }

    public async Task<byte[]?> GetDepartmentQrAsync(Guid deptId)
    {
        var zone = await _locationRepo.GetZoneByIdAsync(deptId);
        if (zone == null)
            return null;

        return GenerateQrCode($"DEPT:{zone.Code}");
    }

    public Task<byte[]> GetDepartmentQrByCodeAsync(string code)
    {
        return Task.FromResult(GenerateQrCode($"DEPT:{code}"));
    }

    public async Task<byte[]?> GetRoomQrAsync(string roomCode)
    {
        var room = await _locationService.GetRoomByCodeAsync(roomCode);
        if (room == null)
            return null;

        return GenerateQrCode($"ROOM:{roomCode}");
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

        return GenerateQrCode(data);
    }

    private static byte[] GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
