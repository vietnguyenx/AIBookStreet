namespace AIBookStreet.API.SearchModel
{
    public class EventSearchRequest
    {
        public string? Key { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ZoneId { get; set; }
    }
}
