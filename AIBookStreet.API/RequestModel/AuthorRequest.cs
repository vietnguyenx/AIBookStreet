namespace AIBookStreet.API.RequestModel
{
    public class AuthorRequest : BaseRequest
    {
        public string AuthorName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
    }
}
