using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Book : BaseEntity
    {
        public string Code { get; set; }
        public string? Title { get; set; }
        public DateTime? PublicationDate { get; set; }
        public decimal? Price { get; set; }
        public string? Languages { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public string? Status { get; set; }

        public Guid? PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<BookAuthor>? BookAuthors { get; set; }
        public virtual ICollection<Inventory>? Inventories { get; set; }
        public virtual ICollection<BookCategory>? BookCategories { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
    }
}
