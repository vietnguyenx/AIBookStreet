namespace AIBookStreet.API.SearchModel
{
    public class OrderSearchRequest
    {
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? StoreId { get; set; }
    }
}
