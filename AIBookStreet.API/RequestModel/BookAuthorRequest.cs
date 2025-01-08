namespace AIBookStreet.API.RequestModel
{
    public class BookAuthorRequest
    {
        public Guid BookId { get; set; }
        public Guid AuthorId { get; set; }
    }
}
