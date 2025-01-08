namespace AIBookStreet.API.RequestModel
{
    public class ZoneRequest : BaseRequest
    {
        public string ZoneName { get; set; }
        public string? Description { get; set; }

        public Guid? StreetId { get; set; }
    }
}
