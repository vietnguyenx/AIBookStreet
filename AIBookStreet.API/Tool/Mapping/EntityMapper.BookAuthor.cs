using AIBookStreet.API.RequestModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;


namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void BookAuthorMapping()
        {
            CreateMap<BookAuthor, BookAuthorModel>().ReverseMap();
            CreateMap<BookAuthorModel, BookAuthorRequest>().ReverseMap();
        }
    }
}
