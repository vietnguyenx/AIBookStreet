namespace AIBookStreet.API.RequestModel
{
    public class EventScheduleRequest
    {
        public Guid Id { get; set; }
        public DateOnly EventDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid EventId { get; set; }
    }
}
