using AutoMapper;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Application.Location.Mappings;

public class LocationMappingProfile : Profile
{
    public LocationMappingProfile()
    {
        CreateMap<Site, SiteDto>();
        CreateMap<Zone, ZoneDto>();
        CreateMap<Building, BuildingDto>();
        CreateMap<Floor, FloorDto>();
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.FloorLevel, opt => opt.MapFrom(src => src.Floor != null ? src.Floor.Level : 0))
            .ForMember(dest => dest.BoundsX, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.X : (double?)null))
            .ForMember(dest => dest.BoundsY, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.Y : (double?)null))
            .ForMember(dest => dest.BoundsWidth, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.Width : (double?)null))
            .ForMember(dest => dest.BoundsHeight, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.Height : (double?)null))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.Color : null))
            .ForMember(dest => dest.Stroke, opt => opt.MapFrom(src => src.RoomGeometry != null ? src.RoomGeometry.Stroke : null));
        
        CreateMap<Building, BuildingDto>().ReverseMap();
        CreateMap<Floor, FloorDto>().ReverseMap();
        CreateMap<RoomGeometry, RoomGeometryDto>();
        CreateMap<ZoneSiteShape, ZoneSiteShapeDto>();
    }
}