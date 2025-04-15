namespace AIBookStreet.API.SearchModel
{
    public class BookSearchRequest
    {
        public string? ISBN { get; set; }
        public string? Title { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? LanguagesList { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
        public List<Guid>? CategoryIds { get; set; }
        public List<Guid>? AuthorIds { get; set; }
    }
}
