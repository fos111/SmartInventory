using SmartInventory.Application.Mobile.Lookup.DTOs;

namespace SmartInventory.Application.Mobile.Lookup.Interfaces;

public interface IMobileLookupService
{
    Task<IEnumerable<MobileCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IEnumerable<MobileDepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<IEnumerable<MobileRoomDto>> GetRoomsByDepartmentAsync(Guid zoneId, CancellationToken ct = default);
    Task<IEnumerable<MobileRoomDto>> GetRoomsByDepartmentCodeAsync(string code, CancellationToken ct = default);
    Task<MobileInventoryStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<IEnumerable<MobileMoveLogEntryDto>> GetMoveLogAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<BarcodeCheckResultDto> CheckBarcodeAsync(string barcode, CancellationToken ct = default);
}
