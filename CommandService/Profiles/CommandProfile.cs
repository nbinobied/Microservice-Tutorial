using AutoMapper;
using CommandService.Dtos;
using CommandService.Models;
using PlatformService.Protos;

namespace CommandService.Profiles
{
    public class CommandProfile : Profile
    {
        public CommandProfile()
        {
            CreateMap<Platform, PlatformDto>();
            CreateMap<CreateCommandDto, Command>();
            CreateMap<Command, CommandDto>();
            CreateMap<PlatformPublishedDto, Platform>()
                .ForMember(destition => destition.ExternalId, option => option.MapFrom(source => source.Id));
            CreateMap<GrpcPlatformModel, Platform>()
                .ForMember(destination => destination.ExternalId, option => option.MapFrom(source => source.PlatformId))
                .ForMember(destination => destination.Name, option => option.MapFrom(source => source.Name))
                .ForMember(destination => destination.Commands, option => option.Ignore());
        }
    }
}
