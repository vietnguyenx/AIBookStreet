using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class StreetRequest
    {
        public Guid Id { get; set; }
        public string StreetName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? BaseImgUrl { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<ImageRequest>? Images { get; set; }
    }
}
