namespace AIBookStreet.API.RequestModel
{
    public class EventRegistrationStatisticRequest
    {
        public Guid EventId { get; set; }
        public bool? IsAttended { get; set; }
    }
}
