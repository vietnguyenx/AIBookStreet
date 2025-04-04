namespace AIBookStreet.API.SearchModel
{
    public class BookSearchRequest
    {
        public string? Code { get; set; }
        public string? Title { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Languages { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
        public Guid? CategoryId { get; set; }
        public Guid? AuthorId { get; set; }
    }

}
