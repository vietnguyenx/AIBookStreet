using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class EventRequest
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = null!;
        public string OrganizerEmail { get; set; } = null!;
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? BaseImgUrl { get; set; }
        public string? VideoLink { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        public bool IsDeleted { get; set; }
        public int Version { get; set; }
        public bool? IsApprove { get; set; }
        public string? Message { get; set; }
        public Guid? UpdateForEventId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public virtual ZoneRequest? Zone { get; set; }
        public virtual ICollection<ImageRequest>? Images { get; set; }
        public virtual ICollection<EventScheduleRequest> EventSchedules { get; set; }
        public int TotalRegistrations { get; set; }
    }
}
