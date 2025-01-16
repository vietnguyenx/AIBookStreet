namespace AIBookStreet.API.RequestModel
{
    public class ImageRequest
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Type { get; set; }
        public string AltText { get; set; } = null!;
        public Guid? EntityId { get; set; }
    }
}
