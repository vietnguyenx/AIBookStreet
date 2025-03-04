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

        public Guid? StreetId { get; set; }
        public virtual Street? Street { get; set; }

        public virtual ICollection<BookStore>? BookStores { get; set; }
        public virtual ICollection<Event>? Events { get; set; }
    }
}
