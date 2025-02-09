namespace AIBookStreet.API.SearchModel
{
    public class BookAuthorSearchRequest
    {
        public string? Key { get; set; }
        public Guid? AuthorId { get; set; }
        public Guid? BookId { get; set; }
    }
}
