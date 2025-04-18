using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void TicketMapping()
        {
            CreateMap<Ticket, TicketModel>().ReverseMap();
            CreateMap<TicketModel, TicketRequest>().ReverseMap();
            CreateMap<Ticket, TicketRequest>().ReverseMap();
        }
    }
}
