using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IEventService
    {
        Task<(long, Event?, string?)> AddAnEvent(EventModel model, List<EventScheduleModel> schedules);
        Task<(long, Event?, string)> ProcessEvent(Guid id, ProcesingEventModel model);
        Task<(long, Event?, string?)> DeleteAnEvent(Guid id);
        Task<(Event?, List<object>, List<object>, List<object>, List<object>, List<object>, int)> GetAnEventById(Guid id);
        Task<(List<Event>?, long)> GetAllEventsPagination(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<List<Event>?> GetEventComing(int number, bool? allowAds);
        Task<List<DateModel>?> GetEventDatesInMonth(int? month);
        Task<List<Event>?> GetEventByDate(DateTime? date);
        Task<List<Event>?> GetRandom(int number);
        Task<object> GetNumberEventInMonth(int month);
        Task<(List<Event>?, long)> GetEventsForCheckin(DateTime? date, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Event>?, long)> GetEventRequests(int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(long, Event?, string)> OpenState(Guid id);
        Task<(long, List<Event>?)> GetHistory(Guid eventId);
        Task<(long, List<Event>?, string?)> GetCreationHistory(int? pageNumber, int? pageSize);
    }
}
