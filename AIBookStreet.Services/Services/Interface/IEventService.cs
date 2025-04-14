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
        Task<(long, Event?)> AddAnEvent(EventModel model);
        Task<(long, Event?)> UpdateAnEvent(Guid id, EventModel model);
        Task<(long, Event?)> DeleteAnEvent(Guid id);
        Task<Event?> GetAnEventById(Guid id);
        Task<(List<Event>?, long)> GetAllEventsPagination(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<List<Event>?> GetEventComing(int number, bool? allowAds);
        Task<List<DateModel>?> GetEventDatesInMonth(int? month);
        Task<List<Event>?> GetEventByDate(DateTime? date);
        Task<List<Event>> GetRandom(int number);
    }
}
