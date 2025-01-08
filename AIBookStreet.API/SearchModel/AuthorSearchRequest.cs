namespace AIBookStreet.API.SearchModel
{
    public class AuthorSearchRequest
    {
        public string AuthorName { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
    }
}
