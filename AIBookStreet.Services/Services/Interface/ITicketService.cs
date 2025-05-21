using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface ITicketService
    {
        Task<(long,Ticket?,string?)> AddATicket(Guid? eventId);
        Task<Ticket?> GetTicket(string email, string passcode);
        Task<Ticket?> GetTicketById(Guid? guid);
        Task<( long, List<Ticket>?)> GetAllTicketOnEvent(Guid eventId);
    }
}
