using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Zone : BaseEntity
    {
        public string ZoneName { get; set; }
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public Guid? StreetId { get; set; }
        public virtual Street? Street { get; set; }

        public virtual ICollection<Store>? Stores { get; set; }
        public virtual ICollection<Event>? Events { get; set; }
    }
}
