using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Order : BaseEntity
    {
        public decimal? TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? Status { get; set; } = null!;
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
