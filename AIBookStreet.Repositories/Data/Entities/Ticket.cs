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
        public Guid? RegistrationId { get; set; }

        public virtual EventRegistration? EventRegistration { get; set; }

    }
}
