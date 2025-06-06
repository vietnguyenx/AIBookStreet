﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<(List<Event>?, long)> GetAllPagination(string? key, bool? allowAds, Guid? zoneId, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Event>?, long)> GetAllPaginationForAdmin(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? zoneId, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<Event?> GetByID(Guid? id);
        Task<List<Event>?> GetEventsComing(int number, bool? allowAds);
        Task<List<DateOnly>?> GetDatesInMonth(int? month);
        Task<List<Event>?> GetByDate(DateTime? date);
        Task<List<Event>?> GetRandom(int number);
        Task<object> GetNumberEventInMonth(int month);
        Task<(List<Event>?, long)> GetEventsForOrganizer(DateTime? date, string? eventName, string? organizerEmail, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<Event>?, long)> GetEventRequests(int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<string?> CheckEventInZone(string start, string end, Guid zoneId);
        Task<Event?> GetLastEventByOrganizerEmail(string email);
        Task<List<Event>?> GetHistory(Guid? eventId);
        Task<List<Event>?> GetCreationHistory(string? email, int? pageNumber, int? pageSize);
        Task<List<Event>?> GetExpiredEvent();
    }
}
