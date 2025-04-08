using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class OrderDetail : BaseEntity
    {
        public Guid? InventoryId { get; set; }
        public Guid? OrderId { get; set; }
        public int Quantity { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Inventory? Inventory { get; set; }
    }
}
