using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class StoreRequest : BaseRequest
    {
        public string StoreName { get; set; }
        public string? Address { get; set; }
        public string? StoreTheme { get; set; }
        public string? BaseImgUrl { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Type { get; set; }

        public Guid? ZoneId { get; set; }

    }
}
