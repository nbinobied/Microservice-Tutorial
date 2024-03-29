﻿using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.Protos;

namespace PlatformService.Profiles
{
    public class PlatformProfile : Profile
    {
        public PlatformProfile()
        {
            CreateMap<Platform, PlatformDto>();
            CreateMap<PlatformCreateDto, Platform>();
            CreateMap<PlatformDto, PlatformPublishedDto>();
            CreateMap<Platform, GrpcPlatformModel>()
                .ForMember(dest => dest.PlatformId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher));
        }
    }
}
