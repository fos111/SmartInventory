using System;
using System.Threading.Tasks;

namespace SmartInventory.Application.Mobile.Reports.Interfaces;

public interface IMobileReportService
{
    /// <summary>
    /// Room fiche PDF — delegates to IReportingService.GetRoomAuditAsync + IPdfReportService.GenerateRoomAudit.
    /// Returns null if room not found.
    /// </summary>
    Task<byte[]?> GetRoomFicheAsync(string roomCode);

    /// <summary>
    /// Room journal PDF — fetches activity log from IReportingService, filters by room code,
    /// generates PDF via PdfReportService. Returns null if room not found.
    /// </summary>
    Task<byte[]?> GetRoomJournalAsync(string roomCode, DateTime? from, DateTime? to);

    /// <summary>
    /// Department QR by GUID ID — looks up zone, returns PNG with format DEPT:{code}.
    /// Returns null if department not found.
    /// </summary>
    Task<byte[]?> GetDepartmentQrAsync(Guid deptId);

    /// <summary>
    /// Department QR by string code — generates PNG with format DEPT:{code} (no DB lookup).
    /// </summary>
    Task<byte[]> GetDepartmentQrByCodeAsync(string code);

    /// <summary>
    /// Room QR by string code — verifies room exists, generates PNG with format ROOM:{code}.
    /// Returns null if room not found.
    /// </summary>
    Task<byte[]?> GetRoomQrAsync(string roomCode);

    /// <summary>
    /// ISet QR — fetches all departments, generates QR with format ISET:{code1},{code2},...
    /// </summary>
    Task<byte[]> GetIsetQrAsync();
    Task InvalidateIsetQrAsync();
}
