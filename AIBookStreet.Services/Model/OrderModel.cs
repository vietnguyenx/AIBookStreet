using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class OrderModel
    {
        public Guid StoreId { get; set; }
        public string PaymentMethod { get; set; }
    }
}
