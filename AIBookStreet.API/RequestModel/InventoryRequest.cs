namespace AIBookStreet.API.RequestModel
{
    public class InventoryRequest
    {
        public Guid BookId { get; set; }
        public Guid BookStoreId { get; set; }
        public int Quantity { get; set; }
        public bool? IsInStock { get; set; }
    }
}
