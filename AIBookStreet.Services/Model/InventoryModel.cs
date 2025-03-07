using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class InventoryModel : BaseModel
    {
        public Guid? EntityId { get; set; }
        public Guid StoreId { get; set; }
        public int Quantity { get; set; }
        public bool? IsInStock { get; set; }

        public BookModel? Book { get; set; }
        public StoreModel Store { get; set; }
        public SouvenirModel? Souvenir { get; set; }
    }
}
