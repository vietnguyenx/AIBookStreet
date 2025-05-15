using AIBookStreet.API.RequestModel;

namespace AIBookStreet.API.ResponseModel
{
    public class EventHistoryResponse
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = null!;
        public string OrganizerEmail { get; set; } = null!;
        public string? Description { get; set; }
        public string? BaseImgUrl { get; set; }
        public string? VideoLink { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        public bool IsDeleted { get; set; }
        public int Version { get; set; }
        public bool? IsApprove { get; set; }
        public string? Message { get; set; }
        public virtual ZoneRequest? Zone { get; set; }
    }
}
