namespace AIBookStreet.API.SearchModel
{
    public class BookSearchRequest
    {
        public string Code { get; set; }
        public string? Title { get; set; }
        public DateTime? PublicationDate { get; set; }
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }
    }
}
