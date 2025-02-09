using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class ZoneRepository(BSDbContext context) : BaseRepository<Zone>(context), IZoneRepository
    {
        public async Task<List<Zone>> GetAll()
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(z => !z.IsDeleted);
            var zones = await queryable.ToListAsync();
            return zones;
        }
        public async Task<(List<Zone>, long)> GetAllPagination(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc == null ? false : (desc == false ? false : true);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(z => !z.IsDeleted);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(z => z.ZoneName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(z.Description) && z.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (streetID != null)
                {
                    queryable = queryable.Where(z => z.StreetId == streetID);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var zones = await queryable.Include(z => z.Street).ToListAsync();

            return (zones, totalOrigin);
        }
        public async Task<(List<Zone>, long)> GetAllPaginationForAdmin(string? key, Guid? streetID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc == null ? false : (desc == false ? false : true);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(z => z.ZoneName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(z.Description) && z.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (streetID != null)
                {
                    queryable = queryable.Where(z => z.StreetId == streetID);
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var zones = await queryable.Include(z => z.Street).ToListAsync();

            return (zones, totalOrigin);
        }
        public async Task<Zone?> GetByID(Guid? id)
        {
            var query = GetQueryable(z => z.Id == id);
            var zone = await query.Include(z => z.BookStores)
                                  .Include(z => z.Street)
                                  .SingleOrDefaultAsync();

            return zone;
        }
    }
}
