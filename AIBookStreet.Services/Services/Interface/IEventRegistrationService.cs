using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IEventRegistrationService
    {
        Task<(long, Ticket?)> AddAnEventRegistration(EventRegistrationModel model);
        //Task<(long, EventRegistration?)> UpdateAnEventRegistration(Guid? id, EventRegistrationModel model);
        //Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id);
        Task<EventRegistration?> GetAnEventRegistrationById(Guid id);
        Task<List<EventRegistration>?> GetAllActiveEventRegistrations(Guid eventId);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>)> Test (Guid eventId);
    }
}
