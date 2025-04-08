using AIBookStreet.API.RequestModel;
using AIBookStreet.API.ResponseModel;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using AutoMapper;

namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper : Profile
    {
        public void InventoryMapping()
        {
            CreateMap<Inventory, InventoryModel>().ReverseMap();
            CreateMap<InventoryModel, InventoryRequest>().ReverseMap();
            CreateMap<Inventory, InventoryResponse>().ReverseMap();
        }
    }
}
