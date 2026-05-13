using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Application.Location.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<Site>> GetFullHierarchyAsync();
        Task<Room?> GetRoomByCodeAsync(string code);
        Task<Room> AddRoomAsync(Room room);
        Task<Floor?> GetFloorByIdAsync(Guid id);
        Task<bool> IsRoomCodeUniqueAsync(string code);
        
        Task<Building> AddBuildingAsync(Building building);
        Task<Floor> AddFloorAsync(Floor floor);
        Task<Zone?> GetZoneByIdAsync(Guid id);
        Task<Building?> GetBuildingByIdAsync(Guid id);
        Task<bool> IsBuildingCodeUniqueAsync(string code);
        Task<Zone?> GetZoneByRoomCodeAsync(string roomCode);

        Task<Room?> GetRoomByIdAsync(Guid id);
        Task<RoomGeometry?> GetRoomGeometryByRoomIdAsync(Guid roomId);
        Task<RoomGeometry> UpsertRoomGeometryAsync(RoomGeometry geometry);
        Task DeleteRoomAsync(Guid roomId);
    }
}