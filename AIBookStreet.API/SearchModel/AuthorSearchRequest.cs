namespace AIBookStreet.API.SearchModel
{
    public class AuthorSearchRequest
    {
        public string? AuthorName { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
