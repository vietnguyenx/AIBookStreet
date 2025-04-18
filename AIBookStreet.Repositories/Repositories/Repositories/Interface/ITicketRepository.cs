using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface ITicketRepository : IBaseRepository<Ticket>
    {
        Task<Ticket?> SearchTicketCode(Guid? eventId, string ticketCode);
        Task<Ticket?> SearchSecretPasscode(Guid? eventId, string secretPasscode);
        Task<Ticket?> GetTicket(string email, string passcode);
        Task<Ticket?> GetByID(Guid id);
    }
}
