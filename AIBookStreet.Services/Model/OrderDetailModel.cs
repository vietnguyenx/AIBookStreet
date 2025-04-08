using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class OrderDetailModel
    {
        public Guid? EntityId { get; set; }
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
    }
}
