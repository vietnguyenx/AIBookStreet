namespace AIBookStreet.API.RequestModel
{
    public class PublisherRequest : BaseRequest
    {
        public string PublisherName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? BaseImgUrl { get; set; }

        public Guid? ManagerId { get; set; }
    }
}
