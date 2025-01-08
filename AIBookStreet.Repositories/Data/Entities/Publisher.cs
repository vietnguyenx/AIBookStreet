using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Publisher : BaseEntity
    {
        public string PublisherName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }

        public Guid? ManagerId { get; set; } 
        public virtual User? Manager { get; set; } 

        public virtual ICollection<Book>? Books { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
    }
}
