using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class TicketModel
    {
        public string TicketCode { get; set; } = null!;
        public string SecretPasscode { get; set; } = null!;
        public Guid? RegistrationId { get; set; }
    }
}
