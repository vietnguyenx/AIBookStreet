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

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class StreetRepository(BSDbContext context) : BaseRepository<Street>(context), IStreetRepository
    {
        public async Task<List<Street>> GetAll()
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(st => !st.IsDeleted);
            var streets = await queryable.ToListAsync();
            return streets;
        }
        public async Task<(List<Street>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(st => !st.IsDeleted);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(st => st.StreetName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(st.Address) && st.Address.ToLower().Trim().Contains(key.ToLower().Trim()))
                                                   || (!string.IsNullOrEmpty(st.Description) && st.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var streets = await queryable
                .Include(st => st.Images).ToListAsync();

            return (streets, totalOrigin);
        }
        public async Task<(List<Street>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(st => st.StreetName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(st.Address) && st.Address.ToLower().Trim().Contains(key.ToLower().Trim()))
                                                   || (!string.IsNullOrEmpty(st.Description) && st.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var streets = await queryable
                .Include(st => st.Images).ToListAsync();

            return (streets, totalOrigin);
        }
        public async Task<Street?> GetByID(Guid id)
        {
            var query = GetQueryable(st => st.Id == id);
            var street = await query.Include(at => at.Zones)
                                    .Include(st => st.Events)
                                  .Include(at => at.Images)
                                  .SingleOrDefaultAsync();

            return street;
        }
        public async Task<Street?> GetByAddress(string address)
        {
            var query = GetQueryable();
            query = query.Where(st => !string.IsNullOrEmpty(st.Address) && st.Address == address);
            var street = await query.Include(at => at.Zones)
                                    .Include(st => st.Events)
                                  .Include(at => at.Images)
                                  .SingleOrDefaultAsync();

            return street;
        }
    }
}
