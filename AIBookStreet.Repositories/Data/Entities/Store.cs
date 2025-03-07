using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Store : BaseEntity
    {
        public string StoreName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public string? BaseImgUrl { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Type { get; set; }

        public Guid? ManagerId { get; set; } 
        public virtual User? Manager { get; set; } 

        public Guid? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }

        public virtual ICollection<Inventory>? Inventories { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
    }
}
