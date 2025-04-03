using AIBookStreet.API.RequestModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void UserStoreMapping()
        {
            CreateMap<UserStore, UserStoreModel>().ReverseMap();
            CreateMap<UserStoreModel, UserStoreRequest>().ReverseMap();
        }
    }
}
