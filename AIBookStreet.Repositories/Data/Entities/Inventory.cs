using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Inventory : BaseEntity
    {
        public Guid? EntityId { get; set; }
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
        public bool? IsInStock { get; set; }

        public virtual Book? Book { get; set; }
        public virtual Souvenir? Souvenir { get; set; }
        public virtual Store Store { get; set; }
    }
}
