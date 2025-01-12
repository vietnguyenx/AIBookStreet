using AIBookStreet.API.RequestModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void BookCategoryMapping()
        {
            CreateMap<BookCategory, BookCategoryModel>().ReverseMap();
            CreateMap<BookCategoryModel, BookCategoryRequest>().ReverseMap();
            CreateMap<BookCategory, BookCategoryRequest>().ReverseMap();
        }
    }
}
