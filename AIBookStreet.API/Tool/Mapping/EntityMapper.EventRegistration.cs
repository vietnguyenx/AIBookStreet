using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void EventRegistrationMapping()
        {
            CreateMap<EventRegistration, EventRegistrationModel>().ReverseMap();
            CreateMap<EventRegistrationModel, EventRegistrationRequest>().ReverseMap();
            CreateMap<Event, EventRegistrationRequest>().ReverseMap();
        }
    }
}
