using AIBookStreet.API.RequestModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void EventScheduleMapping()
        {
            CreateMap<EventSchedule, EventScheduleModel>().ReverseMap();
            CreateMap<EventScheduleModel, EventScheduleRequest>().ReverseMap();
            CreateMap<EventSchedule, EventScheduleRequest>().ReverseMap();
        }
    }
}
