using SmartInventory.Application.Mobile.Home.DTOs;

namespace SmartInventory.Application.Mobile.Home.Interfaces;

public interface IMobileHomeService
{
    Task<HomeSyncDto> GetHomeAsync(Guid userId, CancellationToken ct = default);
}
