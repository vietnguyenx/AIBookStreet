using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Author : BaseEntity
    {
        public string AuthorName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }

        public virtual ICollection<BookAuthor>? BookAuthors { get; set; }
    }
}
