using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class ZoneRequest
    {
        public Guid Id { get; set; }
        public string ZoneName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public virtual StreetRequest? Street { get; set; }
    }
}
