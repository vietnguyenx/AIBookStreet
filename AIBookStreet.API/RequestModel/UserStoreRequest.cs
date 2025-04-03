namespace AIBookStreet.API.RequestModel
{
    public class UserStoreRequest
    {
        public Guid UserId { get; set; }
        public Guid StoreId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }     //Active, Terminated, Expired
        public string? ContractNumber { get; set; } // hop dong thue (neu co)
        public string? Notes { get; set; }
    }
}
