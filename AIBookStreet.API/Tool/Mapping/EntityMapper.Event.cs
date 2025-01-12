using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void EventMapping()
        {
            CreateMap<Event, EventModel>().ReverseMap();
            CreateMap<EventModel, EventRequest>().ReverseMap();
            CreateMap<EventModel, EventSearchRequest>().ReverseMap();
            CreateMap<Event, EventRequest>().ReverseMap();
        }
    }
}
