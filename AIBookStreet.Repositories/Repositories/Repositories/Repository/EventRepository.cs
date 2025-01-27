using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class EventRepository(BSDbContext context) : BaseRepository<Event>(context), IEventRepository
    {
        public async Task<(List<Event>, long)> GetAllPagination(string? key, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ev => ev.EventName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(ev.Description) && ev.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (streetID != null)
                {
                    queryable = queryable.Where(ev => ev.StreetId == streetID);
                }
                if (start != null)
                {
                    queryable = queryable.Where(ev => ev.StartDate > start);
                }
                if (end != null)
                {
                    queryable = queryable.Where(ev => ev.EndDate < end);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Street)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<(List<Event>, long)> GetAllPaginationForAdmin(string? key, DateTime? start, DateTime? end, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ev => ev.EventName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(ev.Description) && ev.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (streetID != null)
                {
                    queryable = queryable.Where(ev => ev.StreetId == streetID);
                }
                if (start != null)
                {
                    queryable = queryable.Where(ev => ev.StartDate > start);
                }
                if (end != null)
                {
                    queryable = queryable.Where(ev => ev.EndDate < end);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Street)
                .ToListAsync();

            return (events, totalOrigin);
        }
        public async Task<Event?> GetByID(Guid id)
        {
            var query = GetQueryable(ev => ev.Id == id);
            var ev = await query.Include(e => e.Street)
                                  .Include(at => at.Images)
                                  .SingleOrDefaultAsync();

            return ev;
        }
        public async Task<List<Event>?> GetEventsComing(int number)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, "StartDate", 1);
            queryable = queryable.Where(ev => !ev.IsDeleted);

            if (queryable.Any())
            {
                queryable = queryable.Where(ev => ev.StartDate > DateTime.Now);
            }
            queryable = GetQueryablePagination(queryable, 1, number);

            var events = await queryable
                .Include(at => at.Images)
                .Include(ev => ev.Street).ToListAsync();

            return events;
        }
    }
}
