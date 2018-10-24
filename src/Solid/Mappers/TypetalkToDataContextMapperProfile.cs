using AutoMapper;

namespace Solid.Mappers
{
    public class TypetalkToDataContextMapperProfile: Profile
    {
        public TypetalkToDataContextMapperProfile()
        {
            CreateMap<Typetalk.Dto.Post, Data.Entities.Post>()
                .ForMember(p => p.Id, cfg => cfg.Ignore())
                .ForMember(p => p.PostId, cfg => cfg.ResolveUsing(p => p.Id));
            CreateMap<Data.Entities.Post, Typetalk.Dto.Post>()
                .ForMember(p => p.Id, cfg => cfg.ResolveUsing(p => p.PostId));

            CreateMap<Typetalk.Dto.Account, Data.Entities.Account>().ReverseMap();
            CreateMap<Typetalk.Dto.Like, Data.Entities.Like>().ReverseMap();
        }
    }
}
