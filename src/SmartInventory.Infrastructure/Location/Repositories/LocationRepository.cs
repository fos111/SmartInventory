using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Location.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Site>> GetFullHierarchyAsync()
        {
            return await _context.Sites
                .Include(s => s.Zones)
                    .ThenInclude(z => z.Buildings)
                        .ThenInclude(b => b.Floors)
                            .ThenInclude(f => f.Rooms)
                                .ThenInclude(r => r.RoomGeometry)
                .ToListAsync();
        }

        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            return await _context.Rooms
                .Include(r => r.Floor)
                    .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.Zone)
                            .ThenInclude(z => z.Site)
                .FirstOrDefaultAsync(r => r.Code.ToLower() == code.ToLower());
        }

        public async Task<Room> AddRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Floor?> GetFloorByIdAsync(Guid id)
        {
            return await _context.Floors
                .Include(f => f.Building)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> IsRoomCodeUniqueAsync(string code)
        {
            return !await _context.Rooms.AnyAsync(r => r.Code == code);
        }

        public async Task<Building> AddBuildingAsync(Building building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task<Floor> AddFloorAsync(Floor floor)
        {
            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();
            return floor;
        }

        public async Task<Zone?> GetZoneByIdAsync(Guid id)
        {
            return await _context.Zones
                .Include(z => z.Site)
                .FirstOrDefaultAsync(z => z.Id == id);
        }

        public async Task<Building?> GetBuildingByIdAsync(Guid id)
        {
            return await _context.Buildings
                .Include(b => b.Zone)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> IsBuildingCodeUniqueAsync(string code)
        {
            return !await _context.Buildings.AnyAsync(b => b.Code == code);
        }

        public async Task<Zone?> GetZoneByRoomCodeAsync(string roomCode)
        {
            return await _context.Rooms
                .Include(r => r.Floor)
                    .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.Zone)
                .Where(r => r.Code.ToLower() == roomCode.ToLower())
                .Select(r => r.Floor!.Building.Zone)
                .FirstOrDefaultAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task<RoomGeometry?> GetRoomGeometryByRoomIdAsync(Guid roomId)
        {
            return await _context.Set<RoomGeometry>()
                .FirstOrDefaultAsync(rg => rg.RoomId == roomId);
        }

        public async Task<RoomGeometry> UpsertRoomGeometryAsync(RoomGeometry geometry)
        {
            var existing = await _context.Set<RoomGeometry>()
                .FirstOrDefaultAsync(rg => rg.RoomId == geometry.RoomId);

            if (existing != null)
            {
                existing.X = geometry.X;
                existing.Y = geometry.Y;
                existing.Width = geometry.Width;
                existing.Height = geometry.Height;
                existing.Color = geometry.Color;
                existing.Stroke = geometry.Stroke;
                existing.ShapeType = geometry.ShapeType;
                existing.Properties = geometry.Properties;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                geometry.Id = Guid.NewGuid();
                geometry.CreatedAt = DateTime.UtcNow;
                _context.Set<RoomGeometry>().Add(geometry);
            }

            await _context.SaveChangesAsync();
            return existing ?? geometry;
        }

        public async Task DeleteRoomAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}