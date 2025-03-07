using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Image : BaseEntity
    {
        public string Url { get; set; } = null!;

        public string? Type { get; set; }

        public string AltText { get; set; } = null!;

        public Guid? EntityId { get; set; }

        public virtual Author? Author { get; set; }

        public virtual Event? Event { get; set; }

        public virtual Publisher? Publisher { get; set; }

        public virtual Store? Store { get; set; }

        public virtual Street? Street { get; set; }

        public virtual User? User { get; set; }

        public virtual Book? Book { get; set; }
        public virtual Souvenir? Souvenir { get; set; }
    }
}
