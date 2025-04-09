using AIBookStreet.API.RequestModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void StoreScheduleMapping()
        {
            CreateMap<StoreSchedule, StoreScheduleModel>()
                .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => src.OpenTime.ToString(@"hh\:mm\:ss")))
                .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => src.CloseTime.ToString(@"hh\:mm\:ss")));

            CreateMap<StoreScheduleModel, StoreSchedule>()
                .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.OpenTime)))
                .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.CloseTime)));

            CreateMap<StoreScheduleRequest, StoreScheduleModel>();
            CreateMap<StoreScheduleModel, StoreScheduleRequest>();
        }
    }
} 