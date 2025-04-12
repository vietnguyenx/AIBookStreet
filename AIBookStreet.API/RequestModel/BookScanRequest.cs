namespace AIBookStreet.API.RequestModel
{
    public class BookScanRequest
    {
        public string ISBN { get; set; }
        public Guid StoreId { get; set; }
        public int Quantity { get; set; } = 1; // Default to 1 book for simple scanning
    }
} 