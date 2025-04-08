namespace AIBookStreet.API.SearchModel
{
    public class OrderSearchRequest
    {
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
