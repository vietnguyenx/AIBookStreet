using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void BookStoreMapping()
        {
            CreateMap<Store, StoreModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Select(i => new Image
                {
                    Id = i.Id,
                    Url = i.Url,
                    Type = i.Type,
                    AltText = i.AltText,
                    EntityId = i.EntityId
                })))
            .ReverseMap();

            CreateMap<StoreModel, StoreRequest>().ReverseMap();
            CreateMap<StoreModel, StoreSearchRequest>().ReverseMap();
        }
    }
}
