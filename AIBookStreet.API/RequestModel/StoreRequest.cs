using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class StoreRequest : BaseRequest
    {
        public string BookStoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public string? BaseImgUrl { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Type { get; set; }

        public Guid? ManagerId { get; set; }

        public Guid? ZoneId { get; set; }

    }
}
