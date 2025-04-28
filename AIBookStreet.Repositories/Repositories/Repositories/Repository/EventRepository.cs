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
        public async Task<(List<Event>, long)> GetAllPagination(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? zoneID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(ev => !ev.IsDeleted && ev.EndDate.Value.Date >= DateTime.Now.Date);
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
                    queryable = queryable.Where(ev => ev.StartDate >= start);
                }
                if (end != null)
                {
                    queryable = queryable.Where(ev => ev.EndDate <= end);
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
        public async Task<(List<Event>, long)> GetAllPaginationForAdmin(string? key, bool? allowAds, DateTime? start, DateTime? end, Guid? zoneID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

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
                    queryable = queryable.Where(ev => ev.StartDate >= start);
                }
                if (end != null)
                {
                    queryable = queryable.Where(ev => ev.EndDate <= end);
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
                                  .SingleOrDefaultAsync();

            return ev;
        }
        public async Task<List<Event>?> GetEventsComing(int number, bool? allowAds)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);
            if (allowAds != null)
            {
                queryable = queryable.Where(e => e.AllowAds == allowAds);
            }

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ev.StartDate >= DateTime.Now);
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
            queryable = base.ApplySort(queryable, "StartDate", 1);

            month = month == null ? DateTime.Now.Month : month;

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => (ev.StartDate.Value.Month == month && ev.StartDate.Value.Year == DateTime.Now.Year) || (ev.EndDate.Value.Month == month && ev.EndDate.Value.Year == DateTime.Now.Year));
            }

            var events = await queryable.ToListAsync();

            var dates = new List<DateOnly>();
            foreach (var evt in events)
            {
                DateTime date = evt.StartDate.Value.Month < month ? new DateTime(DateTime.Now.Year, (int)month, 1) : (DateTime)evt.StartDate;
                DateTime endDate = evt.EndDate.Value.Month > month ? new DateTime(DateTime.Now.Year, (int)month, DateTime.DaysInMonth(DateTime.Now.Year, (int)month)) : (DateTime)evt.EndDate;
                while (date < endDate)
                {
                    var existed = false;
                    var dateConverted = DateOnly.FromDateTime(date);
                    for (int i = 0; i < dates.Count; i++)
                    {
                        if (dates[i] == dateConverted)
                        {
                            existed = true; break;
                        }
                    }
                    if (!existed)
                    {
                        dates.Add(dateConverted);
                    }
                    date = date.AddDays(1);
                }
            }

            return dates;
        }
        public async Task<List<Event>?> GetByDate(DateTime? date)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);
            if (queryable.Any())
            {
                if(date != null)
                {
                    queryable = queryable.Where(ev => ev.StartDate.Value.Date <= date.Value.Date && ev.EndDate.Value.Date >= date.Value.Date);
                }
            }

            var events = await queryable.ToListAsync();
            return events;
        }
        public async Task<List<Event>> GetRandom(int number)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted && ev.AllowAds == true);
            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ev.StartDate >= DateTime.Now);
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
            queryable = queryable.Where(e => !e.IsDeleted && ((e.StartDate.Value.Month == month && e.EndDate.Value.Month == month ) ||
                                                                (e.StartDate.Value.Month == month && e.EndDate.Value.Month == (month + 1)) ||
                                                                (e.StartDate.Value.Month == (month - 1) && e.EndDate.Value.Month == month))
            );
            var eventRegistrations = await queryable.ToListAsync();
            return eventRegistrations.Count;
        }
        public async Task<(List<Event>, long)> GetEventsForStaff(DateTime? date, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            var count = await queryable.CountAsync();
            date = date != null ? date : DateTime.Now;
            queryable = queryable.Where(ev => !ev.IsDeleted && ev.StartDate.Value.Date <= date.Value.Date && ev.EndDate.Value.Date >= date.Value.Date);

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
    }
}
