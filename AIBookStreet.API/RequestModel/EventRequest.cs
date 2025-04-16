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
        public string? BaseImgUrl { get; set; }
        public string? VideoLink { get; set; }
        public bool IsOpen { get; set; }
        public bool AllowAds { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ZoneRequest? Zone { get; set; }
        public virtual ICollection<ImageRequest>? Images { get; set; }
        public List<object>? AgeChart { get; set; }
        public List<object>? GenderChart { get; set; }
        public List<object>? ReferenceChart { get; set; }
        public List<object>? AddressChart { get; set; }
    }
}
