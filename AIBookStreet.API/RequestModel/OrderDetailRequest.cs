using AIBookStreet.API.ResponseModel;
using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class OrderDetailRequest
    {
        public Guid? Id { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Quantity { get; set; }
        public virtual InventoryResponse? Inventory { get; set; }
    }
}
