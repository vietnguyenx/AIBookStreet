using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Souvenir : BaseEntity
    {
        public string SouvenirName { get; set; }
        public string? Description { get; set; }
        public string? BaseImgUrl { get; set; }
        public decimal? Price { get; set; }
        public virtual ICollection<Inventory>? Inventories { get; set; }
        public virtual ICollection<Image>? Images { get; set; }

    }
}
