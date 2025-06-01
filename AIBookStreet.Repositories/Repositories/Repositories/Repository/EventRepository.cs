using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class EventRepository(BSDbContext context) : BaseRepository<Event>(context), IEventRepository
    {
        public async Task<(List<Event>?, long)> GetAllPagination(string? key, bool? allowAds, Guid? zoneID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Include(e => e.EventSchedules);
            queryable = queryable.Where(ev => !ev.IsDeleted 
                                              && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(DateTime.Now) 
                                              && ev.IsApprove.HasValue 
                                              && ev.IsApprove == true 
                                              && ev.IsOpen == true);
            if (allowAds != null)
            {
                queryable = queryable.Where(e => e.AllowAds == allowAds);
            }

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ev => ev.EventName.ToLower().Trim().Contains(key.ToLower().Trim()));
                }
                if (zoneID != null)
                {
                    queryable = queryable.Where(ev => ev.ZoneId == zoneID);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<(List<Event>?, long)> GetAllPaginationForAdmin(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? zoneID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(ev => ev.IsApprove.HasValue && ev.IsApprove == true );
            queryable = queryable.Include(e => e.EventSchedules);

            if (allowAds != null)
            {
                queryable = queryable.Where(e => e.AllowAds == allowAds);
            }

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ev => ev.EventName.ToLower().Trim().Contains(key.ToLower().Trim()));
                }
                if (zoneID != null)
                {
                    queryable = queryable.Where(ev => ev.ZoneId == zoneID);
                }
                if (start != null)
                {
                    queryable = queryable.Where(ev => ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(start.Value));
                }
                if (end != null)
                {
                    queryable = queryable.Where(ev => ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate <= DateOnly.FromDateTime(end.Value));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<Event?> GetByID(Guid? id)
        {
            var query = GetQueryable(ev => ev.Id == id);
            var ev = await query.Include(e => e.Zone)
                                    .ThenInclude(z => z.Street)
                                  .Include(at => at.Images)
                                  .Include(ev => ev.EventSchedules)
                                  .SingleOrDefaultAsync();

            return ev;
        }
        public async Task<List<Event>?> GetEventsComing(int number, bool? allowAds)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "CreatedDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);
            queryable = queryable.Include(e => e.EventSchedules);
            if (allowAds != null)
            {
                queryable = queryable.Where(e => e.AllowAds == allowAds);
            }

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => !ev.IsDeleted && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(DateTime.Now) 
                                                  && ev.IsApprove.HasValue 
                                                  && ev.IsApprove == true 
                                                  && ev.IsOpen == true);
            }
            queryable = GetQueryablePagination(queryable, 1, number);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone).ToListAsync();

            return events;
        }
        public async Task<List<DateOnly>?> GetDatesInMonth(int? month)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "CreatedDate", 1);

            month = month == null ? DateTime.Now.Month : month;
            queryable = queryable.Include(e => e.EventSchedules);

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ((ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Month == month 
                                                    && ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Year == DateTime.Now.Year) 
                                                    || (ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Month == month 
                                                    && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Year == DateTime.Now.Year)) 
                                                    && ev.IsApprove.HasValue 
                                                    && ev.IsApprove == true 
                                                    && ev.IsOpen == true);
            }

            var events = await queryable.ToListAsync();

            var dates = new List<DateOnly>();
            foreach (var evt in events)
            {
                DateOnly date = evt.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Month < month ? new DateOnly(DateTime.Now.Year, (int)month, 1) : evt.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate;
                DateOnly endDate = evt.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Month > month ? new DateOnly(DateTime.Now.Year, (int)month, DateTime.DaysInMonth(DateTime.Now.Year, (int)month)) : evt.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate;
                while (date <= endDate)
                {
                    var existed = false;
                    for (int i = 0; i < dates.Count; i++)
                    {
                        if (dates[i] == date)
                        {
                            existed = true; break;
                        }
                    }
                    if (!existed)
                    {
                        dates.Add(date);
                    }
                    date = date.AddDays(1);
                }
            }

            return dates;
        }
        public async Task<List<Event>?> GetByDate(DateTime? date)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "CreatedDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);
            queryable = queryable.Include(e => e.EventSchedules);
            if (queryable.Any())
            {
                if(date != null)
                {
                    queryable = queryable.Where(ev => ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate <= DateOnly.FromDateTime(date.Value.Date) 
                                                      && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(date.Value.Date) 
                                                      && ev.IsApprove.HasValue 
                                                      && ev.IsApprove == true 
                                                      && ev.IsOpen == true);
                }
            }

            var events = await queryable.ToListAsync();
            return events;
        }
        public async Task<List<Event>?> GetRandom(int number)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "CreatedDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted && ev.AllowAds == true);
            queryable = queryable.Include(e => e.EventSchedules);
            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(DateTime.Now) 
                                                  && ev.IsApprove.HasValue && ev.IsApprove == true && ev.IsOpen == true);
                queryable = queryable.OrderBy(x => Guid.NewGuid());
            }
            var events = await queryable.Take(number).ToListAsync();
            return events;
        }
        public async Task<object> GetNumberEventInMonth(int month)
        {
            var monthData = await CountNumberEventInMonth(month);
            var lastMonthData = await CountNumberEventInMonth(month - 1);
            return new
            {
                success = true,
                total = monthData,
                change = monthData - lastMonthData,
                direction = monthData - lastMonthData > 0 ? "increase" : monthData - lastMonthData == 0 ? "maintain" : "descrease"
            };
        }

        public async Task<int> CountNumberEventInMonth (int month)
        {
            var queryable = GetQueryable();
            queryable = queryable.Include(e => e.EventSchedules);
            queryable = queryable.Where(e => !e.IsDeleted && ((e.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Month == month && e.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Month == month && e.IsApprove.HasValue && e.IsApprove == true && e.IsOpen == true) 
                                                          || (e.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Month == month && e.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Month == (month + 1) && e.IsApprove.HasValue && e.IsApprove == true && e.IsOpen == true)
                                                          || (e.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate.Month == (month - 1) && e.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate.Month == month) && e.IsApprove.HasValue && e.IsApprove == true && e.IsOpen == true)
            );
            var eventRegistrations = await queryable.ToListAsync();
            return eventRegistrations.Count;
        }
        public async Task<(List<Event>?, long)> GetEventsForOrganizer(DateTime? date, string? eventName, string? email, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            queryable = queryable.Include(e => e.EventSchedules);
            var count = await queryable.CountAsync();
            date = date != null ? date : DateTime.Now;
            queryable = queryable.Where(ev => !ev.IsDeleted 
                                            && ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate <= DateOnly.FromDateTime(date.Value.Date) 
                                            && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate >= DateOnly.FromDateTime(date.Value.Date) 
                                            && ev.IsApprove.HasValue 
                                            && ev.IsApprove == true 
                                            && ev.IsOpen == true);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    queryable = queryable.Where(ev => ev.EventName.ToLower().Contains(eventName.ToLower()));
                }
                if (!string.IsNullOrEmpty(email))
                {
                    queryable = queryable.Where(ev => ev.OrganizerEmail == email);
                }
            }

            var totalOrigin = queryable.Count();
            var x = await queryable.CountAsync();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<(List<Event>?, long)> GetEventRequests(int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(ev => !ev.IsDeleted && !ev.IsApprove.HasValue);

            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone)
                .Include(ev => ev.EventSchedules)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<string?> CheckEventInZone(string start, string end, Guid zoneId)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            var startDate = DateOnly.Parse(start);
            var endDate = DateOnly.Parse(end);
            queryable = queryable.Where(ev => !ev.IsDeleted);
            queryable = queryable.Include(e => e.EventSchedules);
            var date = new List<DateOnly>();
            while(startDate <= endDate)
            {
                var evtCount = queryable.Where(ev => ev.EventSchedules.OrderBy(es => es.EventDate).FirstOrDefault().EventDate <= startDate && ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate >= startDate && ev.IsApprove.HasValue && ev.IsApprove == true);
                if (await evtCount.AnyAsync())
                {
                    date.Add(startDate);
                }
                startDate = startDate.AddDays(1);
            }
            var message = "Đã có sự kiện diễn ra ở khu vực này trong ngày: ";
            if (date != null && date.Count > 0)
            {
                foreach (var e in date)
                {
                    message += e.ToString("dd/MM") + ", ";
                }
            }

            return date.Count switch { 
                0 => null,
                _ => message
            };
        }
        public async Task<Event?> GetLastEventByOrganizerEmail(string email)
        {
            var query = GetQueryable(ev => ev.OrganizerEmail == email);
            var ev = await query.ToListAsync();

            return ev.OrderByDescending(e => e.CreatedDate).FirstOrDefault();
        }
        public async Task<List<Event>?> GetHistory(Guid? eventId)
        {
            if (eventId == null) { return null; }
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "CreatedDate", 1);
            if (queryable.Any())
            {
                if (eventId != null)
                {
                    queryable = queryable.Where(e => (e.UpdateForEventId.HasValue && e.UpdateForEventId == eventId) || e.Id == eventId);
                }
            }
            var events = await queryable.OrderBy(e => e.Version)
                                        .Include(e => e.Zone)
                                            .ThenInclude(z => z.Street)
                                        .ToListAsync();
            return events;
        }
        public async Task<List<Event>?> GetCreationHistory(string? email, int? pageNumber, int? pageSize)
        {
            if (email == null) { return null; }
            var queryable = GetQueryable();

            if (queryable.Any())
            {
                if (email != null)
                {
                    queryable = queryable.Where(e => e.OrganizerEmail.ToLower().Equals(email.ToLower()));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable.OrderByDescending(e => e.CreatedDate)
                                        .Include(e => e.Zone)
                                            .ThenInclude(z => z.Street)
                                        .Include(e => e.EventSchedules)
                                        .ToListAsync();
            return events;
        }
        public async Task<List<Event>?> GetExpiredEvent()
        {
            var queryable = GetQueryable();
            queryable = queryable.Include(e => e.EventSchedules);

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ev.EventSchedules.OrderByDescending(es => es.EventDate).FirstOrDefault().EventDate < DateOnly.FromDateTime(DateTime.Now)
                                                  && ev.IsApprove.HasValue
                                                  && ev.IsApprove == true
                                                  && ev.IsOpen == true);
            }

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Zone).ToListAsync();

            return events;
        }
    }
}
