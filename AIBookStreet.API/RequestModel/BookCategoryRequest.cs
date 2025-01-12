using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class BookCategoryRequest 
    {
        public Guid Id { get; set; }
        public bool? IsDeleted { get; set; }
        public virtual Book Book { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
