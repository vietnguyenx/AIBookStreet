namespace AIBookStreet.API.SearchModel
{
    public class OrderDetailSearchRequest
    {
        public Guid? OrderId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? EntityId { get; set; }
    }
}
