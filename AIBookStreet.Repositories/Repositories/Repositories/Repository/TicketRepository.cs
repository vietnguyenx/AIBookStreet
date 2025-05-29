using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class TicketRepository(BSDbContext context) : BaseRepository<Ticket>(context), ITicketRepository
    {
        public async Task<Ticket?> SearchTicketCode(Guid? eventId, string ticketCode)
        {
            var query = GetQueryable();
            query = query.Include(t => t.EventRegistrations)
                        .Where(t => t.EventRegistrations.FirstOrDefault().EventId == eventId && t.TicketCode == ticketCode);
            var ticket = await query.SingleOrDefaultAsync();
            return ticket;
        }
        public async Task<Ticket?> SearchSecretPasscode(Guid? eventId, string secretPasscode)
        {
            var query = GetQueryable();
            query = query.Include(t => t.EventRegistrations);
            if (query.Any())
            {
                query = query.Where(t => t.EventRegistrations.FirstOrDefault().EventId == eventId && t.SecretPasscode == secretPasscode);
            }
            var ticket = await query.FirstOrDefaultAsync();
            return ticket;
        }
        public async Task<Ticket?> GetByID(Guid? id)
        {
            if (id == null) { return null; }
            var query = GetQueryable(t=> t.Id == id);
            var ticket = await query.Include(t => t.EventRegistrations)
                                        .ThenInclude(er => er.Event)
                                            .ThenInclude(e => e.Zone)
                                                .ThenInclude(z => z.Street)
                                    .Include(t => t.EventRegistrations)
                                        .ThenInclude(er => er.Event)
                                            .ThenInclude(e => e.EventSchedules).SingleOrDefaultAsync();
            return ticket;
        }
        public async Task<Ticket?> GetTicket(string email, string passcode)
        {
            var query = GetQueryable();
            query = query.Include(t => t.EventRegistrations).ThenInclude(er => er.Event).ThenInclude(e => e.Zone).ThenInclude(z => z.Street)
                         .Include(t => t.EventRegistrations).ThenInclude(er => er.Event).ThenInclude(e => e.EventSchedules);
            if (query.Any())
            {
                query = query.Where(t => t.EventRegistrations != null && t.EventRegistrations.FirstOrDefault() != null && t.EventRegistrations.FirstOrDefault().RegistrantEmail == email && t.SecretPasscode == passcode);
            }
            var ticket = await query.FirstOrDefaultAsync();
            return ticket;
        }
        public async Task<List<Ticket>> GetAllTicketOnEvent(Guid eventId)
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(z => !z.IsDeleted);
            queryable = queryable.Include(t => t.EventRegistrations).ThenInclude(er => er.Event).ThenInclude(e => e.Zone). ThenInclude(z => z.Street)
                                 .Include(t => t.EventRegistrations).ThenInclude(er => er.Event).ThenInclude(e => e.EventSchedules);
            queryable = queryable.Where(t => t.EventRegistrations.FirstOrDefault().EventId == eventId);
            var tickets = await queryable.ToListAsync();
            return tickets;
        }
    }
}
