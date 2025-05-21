using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class EventRegistrationModel
    {
        public string RegistrantName { get; set; } = null!;
        public string RegistrantEmail { get; set; } = null!;
        public string RegistrantPhoneNumber { get; set; } = null!;
        public string RegistrantAgeRange { get; set; } = null!;
        public string RegistrantGender { get; set; } = null!;
        public string RegistrantAddress { get; set; } = null!;
        public string ReferenceSource { get; set; } = null!;
        public bool HasAttendedBefore { get; set; }
        public Guid? EventId { get; set; }
        public List<string> DateToAttends { get; set; } = [];
    }
}
