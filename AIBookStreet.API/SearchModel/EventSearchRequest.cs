namespace AIBookStreet.API.SearchModel
{
    public class EventSearchRequest
    {
        public string EventName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
