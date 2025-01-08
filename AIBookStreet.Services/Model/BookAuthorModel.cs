using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class BookAuthorModel : BaseModel
    {
        public Guid BookId { get; set; }
        public Guid AuthorId { get; set; }
        public BookModel Book { get; set; }
        public AuthorModel Author { get; set; }
    }
}
