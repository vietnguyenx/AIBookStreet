namespace AIBookStreet.API.SearchModel
{
    public class BookStoreSearchRequest
    {
        public string? BookStoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
    }
}
