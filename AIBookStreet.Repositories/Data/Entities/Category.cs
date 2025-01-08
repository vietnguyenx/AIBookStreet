using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Category : BaseEntity
    {
        public string CategoryName { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<BookCategory>? BookCategories { get; set; }
    }
}
