using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Street : BaseEntity
    {
        public string StreetName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Zone>? Zones { get; set; }
        public virtual ICollection<Event>? Events { get; set; }
    }
}
