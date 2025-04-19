using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class EventRegistration: BaseEntity
    {
        public string RegistrantName { get; set; }
        public string RegistrantEmail { get; set; }
        public string RegistrantPhoneNumber { get; set; }
        public string RegistrantAgeRange { get; set; }
        public string RegistrantGender { get; set; }   
        public string? RegistrantAddress { get; set; }
        public string? ReferenceSource { get; set; }
        public bool HasAttendedBefore { get; set; }
        public Guid? EventId { get; set;}
        public virtual Event? Event { get; set; }
        public virtual Ticket? Ticket { get; set; }
    }
}
