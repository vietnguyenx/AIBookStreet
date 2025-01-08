using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookCategoryModel : BaseModel
    {
        public Guid BookId { get; set; }
        public Guid CategoryId { get; set; }
        public BookModel Book { get; set; }
        public CategoryModel Category { get; set; }
    }
}
