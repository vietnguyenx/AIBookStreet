using AIBookStreet.API.RequestModel;
using AIBookStreet.API.SearchModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void OrderMapping()
        {
            CreateMap<Order, OrderModel>().ReverseMap();
            CreateMap<OrderModel, OrderRequest>().ReverseMap();
            CreateMap<OrderModel, OrderSearchRequest>().ReverseMap();
            CreateMap<Order, OrderRequest>().ReverseMap();
        }
    }
}
