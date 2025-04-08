namespace AIBookStreet.API.RequestModel
{
    public class SouvenirRequest
    {
        public Guid Id { get; set; }
        public string SouvenirName { get; set; } = null!;
        public string? Description { get; set; }
        public string? BaseImgUrl { get; set; }
        public decimal? Price { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
