using AutoMapper;
using SmartInventory.Application.Asset.DTOs;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Application.Asset.Mappings;

public class AssetMappingProfile : Profile
{
    public AssetMappingProfile()
    {
        CreateMap<AssetEntity, AssetDto>()
            .ForMember(dest => dest.HasDiscrepancy, 
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DetectedRoomCode) && src.CurrentRoomCode != src.DetectedRoomCode))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.DeletedAt != null))
            .ForMember(dest => dest.ZoneCode, opt => opt.Ignore())
            .ForMember(dest => dest.ZoneName, opt => opt.Ignore());

        CreateMap<CreateAssetDto, AssetEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DetectedRoomCode, opt => opt.Ignore())
            .ForMember(dest => dest.LastSeen, opt => opt.Ignore())
            .ForMember(dest => dest.RfidTagId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<UpdateAssetDto, AssetEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AssetTag, opt => opt.Ignore())
            .ForMember(dest => dest.DetectedRoomCode, opt => opt.Ignore())
            .ForMember(dest => dest.LastSeen, opt => opt.Ignore())
            .ForMember(dest => dest.RfidTagId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<AssetEntity, AssetReconciliationDto>()
            .ForMember(dest => dest.HasDiscrepancy, 
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DetectedRoomCode) && src.CurrentRoomCode != src.DetectedRoomCode))
            .ForMember(dest => dest.DiscrepancyType, opt => opt.MapFrom(src => 
                src.DetectedRoomCode == null ? "NotDetected" : "Moved"));
    }
}