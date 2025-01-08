namespace AIBookStreet.API.RequestModel
{
    public class EventRequest : BaseRequest
    {
        public string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Guid? StreetId { get; set; }
    }
}
