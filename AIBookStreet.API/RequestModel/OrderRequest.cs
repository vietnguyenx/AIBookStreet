namespace AIBookStreet.API.RequestModel
{
    public class OrderRequest
    {
        public Guid? Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public StoreRequest? Store { get; set; }
        public virtual ICollection<OrderDetailRequest>? OrderDetails { get; set; }
    }
}
