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
        Task<(long, EventRegistration?,string?)> AddAnEventRegistration(EventRegistrationModel model);
        Task<int> SendEmai(Ticket? ticket);
        Task<(long, List<EventRegistration>?)> CheckAttend(List<CheckAttendModel> models);
        //Task<(long, EventRegistration?)> DeleteAnEventRegistration(Guid id);
        Task<EventRegistration?> GetAnEventRegistrationById(Guid id);
        Task<(long, List<EventRegistration>?)> GetAllActiveEventRegistrations(Guid eventId, string? searchKey);
        Task<(List<object>, List<object>, List<object>, List<object>, List<object>, int, int)> Test (Guid eventId);
    }
}
