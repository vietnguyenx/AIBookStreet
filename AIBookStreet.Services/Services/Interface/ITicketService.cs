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
        Task<Ticket?> AddATicket(TicketModel model);
        Task<Ticket?> GetTicket(string email, string passcode);
    }
}
