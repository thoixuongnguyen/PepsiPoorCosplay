using AutoMapper;
using PepsiCompetitive.Modules.Beats.Entities;
using PepsiCompetitive.Modules.Beats.Requests;
using PepsiCompetitive.Modules.Players.Entities;
using PepsiCompetitive.Modules.Players.Requests;
using PepsiCompetitive.Modules.Videos.Entities;
using PepsiCompetitive.Modules.Videos.Requests;

namespace PepsiCompetitive.App.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PlayerSignUpRequest, Player>().ForMember(dest => dest.Avatar, opt => opt.Ignore());
            CreateMap<PlayerLoginRequest, Player>().ForAllMembers(x => x.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PlayerUpdateRequest, Player>();

            CreateMap<BeatStoreRequest, Beat>();
            CreateMap<BeatUpdateRequest, Beat>();

            CreateMap<VideoUploadRequest, Video>();
            CreateMap<VideoUpdateRequest, Video>();
        }
    }
}
