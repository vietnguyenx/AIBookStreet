using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class TicketModel
    {
        public string TicketCode { get; set; }
        public string SecretPasscode { get; set; }
        public Guid? RegistrationId { get; set; }
    }
}
