using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Ticket : BaseEntity
    {
        public string TicketCode { get; set; }
        public string SecretPasscode { get; set; }
        public virtual ICollection<EventRegistration>? EventRegistrations { get; set; }

    }
}
