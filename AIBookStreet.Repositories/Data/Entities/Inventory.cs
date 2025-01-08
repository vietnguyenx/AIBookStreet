using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Inventory : BaseEntity
    {
        public Guid BookId { get; set; }
        public Guid BookStoreId { get; set; }
        public int Quantity { get; set; }
        public bool? IsInStock { get; set; }

        public virtual Book Book { get; set; }
        public virtual BookStore BookStore { get; set; }
    }
}
