using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Domain.Notification.Enums;

namespace SmartInventory.Application.Location.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _repository;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public LocationService(
            ILocationRepository repository,
            IActivityLogService activityLogService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _repository = repository;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<HierarchyDto> GetHierarchyAsync()
        {
            var sites = await _repository.GetFullHierarchyAsync();
            var site = sites.FirstOrDefault();
            if (site == null)
                return new HierarchyDto { Site = null! };

            return new HierarchyDto
            {
                Site = _mapper.Map<SiteDto>(site)
            };
        }

        public async Task<RoomDto?> GetRoomByCodeAsync(string code)
        {
            var room = await _repository.GetRoomByCodeAsync(code);
            return room == null ? null : _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, Guid userId)
        {
            if (!await _repository.IsRoomCodeUniqueAsync(dto.Code))
                throw new InvalidOperationException($"Room code '{dto.Code}' already exists.");

            var floor = await _repository.GetFloorByIdAsync(dto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {dto.FloorId} not found.");

            var room = new Room
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                FloorId = dto.FloorId
            };

            var created = await _repository.AddRoomAsync(room);

            if (dto.BoundsX.HasValue || dto.BoundsY.HasValue ||
                dto.BoundsWidth.HasValue || dto.BoundsHeight.HasValue ||
                dto.Color != null || dto.Stroke != null)
            {
                var geometry = new RoomGeometry
                {
                    RoomId = created.Id,
                    X = dto.BoundsX ?? 0,
                    Y = dto.BoundsY ?? 0,
                    Width = dto.BoundsWidth ?? 200,
                    Height = dto.BoundsHeight ?? 200,
                    Color = dto.Color ?? "#e2e8f0",
                    Stroke = dto.Stroke ?? "#94a3b8",
                };

                await _repository.UpsertRoomGeometryAsync(geometry);
            }

            await _activityLogService.TrackFacilityChangeAsync(
                "Created", "Room", created.Code, created.Name, null, userId);

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.FacilityRoomCreated,
                Type = NotificationType.Info,
                Title = "Room Created",
                Message = $"{created.Name} ({created.Code}) was created on floor {floor.Level}",
                TargetRole = UserRole.Supervisor
            });

            // Re-fetch with geometry included for the response
            var updatedRoom = await _repository.GetRoomByIdAsync(created.Id);
            return _mapper.Map<RoomDto>(updatedRoom);
        }

        public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto dto, Guid userId)
        {
            if (!await _repository.IsBuildingCodeUniqueAsync(dto.Code))
                throw new InvalidOperationException($"Building code '{dto.Code}' already exists.");

            var zone = await _repository.GetZoneByIdAsync(dto.ZoneId);
            if (zone == null)
                throw new ArgumentException($"Zone with ID {dto.ZoneId} not found.");

            var building = new Building
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                ZoneId = dto.ZoneId
            };

            var created = await _repository.AddBuildingAsync(building);

            await _activityLogService.TrackFacilityChangeAsync(
                "Created", "Building", created.Code, created.Name, null, userId);

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.FacilityBuildingCreated,
                Type = NotificationType.Info,
                Title = "Building Created",
                Message = $"{created.Name} ({created.Code}) was created — Zone: {zone.Name} ({zone.Code})",
                TargetRole = UserRole.Supervisor
            });

            return _mapper.Map<BuildingDto>(created);
        }

        public async Task<FloorDto> CreateFloorAsync(CreateFloorDto dto, Guid userId)
        {
            var building = await _repository.GetBuildingByIdAsync(dto.BuildingId);
            if (building == null)
                throw new ArgumentException($"Building with ID {dto.BuildingId} not found.");

            var zone = await _repository.GetZoneByIdAsync(building.ZoneId);

            var floor = new Floor
            {
                Level = dto.Level,
                Description = dto.Description,
                BuildingId = dto.BuildingId
            };

            var created = await _repository.AddFloorAsync(floor);

            await _activityLogService.TrackFacilityChangeAsync(
                "Created", "Floor", dto.BuildingId.ToString(), $"Level {dto.Level}", null, userId);

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.FacilityFloorCreated,
                Type = NotificationType.Info,
                Title = "Floor Created",
                Message = $"Floor {created.Level} was created in {building.Name} ({building.Code}) — Zone: {zone.Name} ({zone.Code})",
                TargetRole = UserRole.Supervisor
            });

            return _mapper.Map<FloorDto>(created);
        }

        public async Task<RoomGeometryDto> UpdateRoomGeometryAsync(Guid roomId, UpdateRoomGeometryDto dto, Guid userId)
        {
            var room = await _repository.GetRoomByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");

            var geometry = new RoomGeometry
            {
                RoomId = roomId,
                X = dto.X ?? 0,
                Y = dto.Y ?? 0,
                Width = dto.Width ?? 200,
                Height = dto.Height ?? 200,
                Color = dto.Color ?? "#e2e8f0",
                Stroke = dto.Stroke ?? "#94a3b8",
            };

            var saved = await _repository.UpsertRoomGeometryAsync(geometry);

            await _activityLogService.TrackFacilityChangeAsync(
                "Updated", "RoomGeometry", room.Code, room.Name, null, userId);

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.FacilityRoomUpdated,
                Type = NotificationType.Info,
                Title = "Room Updated",
                Message = $"{room.Name} ({room.Code}) was updated",
                TargetRole = UserRole.Supervisor
            });

            return _mapper.Map<RoomGeometryDto>(saved);
        }

        public async Task DeleteRoomAsync(Guid roomId, Guid userId)
        {
            var room = await _repository.GetRoomByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");

            await _repository.DeleteRoomAsync(roomId);

            await _activityLogService.TrackFacilityChangeAsync(
                "Deleted", "Room", room.Code, room.Name, null, userId);

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                EventType = NotificationEventType.FacilityRoomDeleted,
                Type = NotificationType.Warning,
                Title = "Room Deleted",
                Message = $"{room.Name} ({room.Code}) was deleted",
                TargetRole = UserRole.Supervisor
            });
        }

        public async Task<List<RoomGeometryDto>> BatchUpdateRoomGeometriesAsync(BatchUpdateRoomGeometriesDto dto, Guid userId)
        {
            var results = new List<RoomGeometryDto>();

            foreach (var update in dto.Updates)
            {
                var room = await _repository.GetRoomByIdAsync(update.RoomId);
                if (room == null)
                    throw new KeyNotFoundException($"Room with ID {update.RoomId} not found.");

                var geometry = new RoomGeometry
                {
                    RoomId = update.RoomId,
                    X = update.X ?? 0,
                    Y = update.Y ?? 0,
                    Width = update.Width ?? 200,
                    Height = update.Height ?? 200,
                    Color = update.Color ?? "#e2e8f0",
                    Stroke = update.Stroke ?? "#94a3b8",
                };

                var saved = await _repository.UpsertRoomGeometryAsync(geometry);
                results.Add(_mapper.Map<RoomGeometryDto>(saved));
            }

            if (dto.Updates.Count > 0)
            {
                // Fetch room names for the activity log
                var roomIds = dto.Updates.Select(u => u.RoomId).ToList();
                var rooms = await _repository.GetRoomsByIdsAsync(roomIds);
                var roomNames = rooms.Select(r => r.Name).ToList();

                var roomList = string.Join(", ", roomNames.Take(5));
                if (roomNames.Count > 5)
                    roomList += $" and {roomNames.Count - 5} more";

                // Determine what type of change occurred
                var changeTypes = new List<string>();
                if (dto.Updates.Any(u => u.X.HasValue || u.Y.HasValue))
                    changeTypes.Add("positions");
                if (dto.Updates.Any(u => u.Width.HasValue || u.Height.HasValue))
                    changeTypes.Add("sizes");
                if (dto.Updates.Any(u => u.Color != null || u.Stroke != null))
                    changeTypes.Add("appearance");

                var changeDesc = changeTypes.Count > 0
                    ? string.Join("/", changeTypes) + " updated"
                    : "updated";

                await _activityLogService.TrackFacilityChangeAsync(
                    "Updated", "Room", "layout",
                    $"{roomNames.Count} room{(roomNames.Count > 1 ? "s" : "")}",
                    $"{changeDesc}: {roomList}", userId);
            }

            return results;
        }
    }
}