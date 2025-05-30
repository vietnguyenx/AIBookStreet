namespace AIBookStreet.API.RequestModel
{
    public class ContractEmailRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StoreType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string ContractNumber { get; set; }
        public string ContractFileUrl { get; set; }
        public string Notes { get; set; }
        public string BaseImgUrl { get; set; }
    }
}
