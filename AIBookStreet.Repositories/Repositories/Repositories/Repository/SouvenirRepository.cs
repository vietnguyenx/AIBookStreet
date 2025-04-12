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
    public class SouvenirRepository(BSDbContext context) : BaseRepository<Souvenir>(context), ISouvenirRepository
    {
        public async Task<(List<Souvenir>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(c => !c.IsDeleted);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(c => c.SouvenirName.ToLower().Trim().Contains(key.ToLower().Trim()));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var categories = await queryable.ToListAsync();

            return (categories, totalOrigin);
        }
        public async Task<(List<Souvenir>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(c => c.SouvenirName.ToLower().Trim().Contains(key.ToLower().Trim()));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var categories = await queryable.ToListAsync();

            return (categories, totalOrigin);
        }
        public async Task<Souvenir?> GetByID(Guid? id)
        {
            var query = GetQueryable(c => c.Id == id);
            var category = await query.Include(c => c.Inventories)
                                       .Include(bc => bc.Images)
                                  .SingleOrDefaultAsync();

            return category;
        }
    }
}
