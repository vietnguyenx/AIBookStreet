namespace AIBookStreet.API.RequestModel
{
    public class StreetRequest : BaseRequest
    {
        public string StreetName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}
