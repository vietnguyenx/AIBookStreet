namespace AIBookStreet.API.ResponseModel
{
    public class BookResponse
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? BaseImgUrl { get; set; }
    }
}
