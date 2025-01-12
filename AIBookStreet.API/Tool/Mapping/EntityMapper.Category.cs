using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;


namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void CategoryMapping()
        {
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<CategoryModel, CategoryRequest>().ReverseMap();
            CreateMap<CategoryModel, CategorySearchRequest>().ReverseMap();
            CreateMap<Category, CategoryRequest>().ReverseMap();
        }
    }
}
