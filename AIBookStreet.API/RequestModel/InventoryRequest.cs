namespace AIBookStreet.API.RequestModel
{
    public class InventoryRequest
    {
        public Guid? EntityId { get; set; }
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
        public bool? IsInStock { get; set; }
    }
}
