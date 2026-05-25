using System;
using System.Threading.Tasks;
using SmartInventory.Application.Location.DTOs;

namespace SmartInventory.Application.Location.Interfaces
{
    public interface ILocationService
    {
        Task<HierarchyDto> GetHierarchyAsync();
        Task<RoomDto?> GetRoomByCodeAsync(string code);
        Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, Guid userId);
        Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto dto, Guid userId);
        Task<FloorDto> CreateFloorAsync(CreateFloorDto dto, Guid userId);

        Task<RoomGeometryDto> UpdateRoomGeometryAsync(Guid roomId, UpdateRoomGeometryDto dto, Guid userId);
        Task DeleteRoomAsync(Guid roomId, Guid userId);
        Task<List<RoomGeometryDto>> BatchUpdateRoomGeometriesAsync(BatchUpdateRoomGeometriesDto dto, Guid userId);

        Task<SiteConfigDto> GetSiteConfigAsync();
        Task<SiteConfigDto> UpdateSiteConfigAsync(UpdateSiteConfigDto dto, Guid userId);
        Task<ZoneSiteShapeDto> CreateZoneSiteShapeAsync(CreateZoneSiteShapeDto dto, Guid userId);
        Task<ZoneSiteShapeDto> UpdateZoneSiteShapeAsync(Guid id, UpdateZoneSiteShapeDto dto, Guid userId);
        Task DeleteZoneSiteShapeAsync(Guid id, Guid userId);
    }
}