using AIBookStreet.API.ResponseModel;
using AIBookStreet.Repositories.Data.Entities;

namespace AIBookStreet.API.RequestModel
{
    public class BookCategoryRequest 
    {
        public Guid Id { get; set; }
        public bool? IsDeleted { get; set; }
        public virtual BookResponse Book { get; set; } = null!;
        public virtual CategoryRequest Category { get; set; } = null!;
    }
}
