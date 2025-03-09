namespace AIBookStreet.API.SearchModel
{
    public class ImageSearchRequest
    {
        public string? Type { get; set; }
        public string? AltText { get; set; }
        public Guid? EntityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
