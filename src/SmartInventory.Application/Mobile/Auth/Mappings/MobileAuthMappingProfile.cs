using AutoMapper;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Helpers;
using SmartInventory.Domain.Auth.Entities;

namespace SmartInventory.Application.Mobile.Auth.Mappings;

public class MobileAuthMappingProfile : Profile
{
    public MobileAuthMappingProfile()
    {
        CreateMap<User, MobileUserDto>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Username))
            .ForMember(
                dest => dest.Role,
                opt => opt.MapFrom(src =>
                    src.Role.HasValue
                        ? MobileRoleMapper.MapToMobile(src.Role.Value)
                        : MobileRoleMapper.DefaultMobileRole))
            .ForMember(
                dest => dest.Avatar,
                opt => opt.MapFrom(src => src.AvatarUrl));

        CreateMap<User, RegisterResultDto>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(
                dest => dest.RequiresVerification,
                opt => opt.MapFrom(_ => true));
    }
}
