namespace AIBookStreet.API.SearchModel
{
    public class BookCategorySearchRequest
    {
        public string? Key { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BookId { get; set; }
    }
}
