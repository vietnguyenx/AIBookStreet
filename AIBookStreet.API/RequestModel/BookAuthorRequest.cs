using AIBookStreet.API.ResponseModel;
using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class BookAuthorRequest
    {
        public Guid Id { get; set; }
        public bool? IsDeleted { get; set; }
        public virtual BookResponse Book { get; set; } = null!;
        public virtual AuthorRequest Author { get; set; } = null!;
    }
}
