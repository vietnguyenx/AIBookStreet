using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void SouvenirMapping()
        {
            CreateMap<Souvenir, SouvenirModel>().ReverseMap();
            CreateMap<SouvenirModel, SouvenirRequest>().ReverseMap();
            CreateMap<SouvenirModel, SouvenirSearchRequest>().ReverseMap();
            CreateMap<Souvenir, SouvenirRequest>().ReverseMap();
        }
    }
}
