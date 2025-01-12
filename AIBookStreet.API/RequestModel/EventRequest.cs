using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class EventRequest
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? StreetId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Image>? Images { get; set; }
    }
}
