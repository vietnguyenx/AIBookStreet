using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class BookAuthorRequest
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid AuthorId { get; set; }
        public virtual Book Book { get; set; } = null!;
        public virtual Author Author { get; set; } = null!;
    }
}
