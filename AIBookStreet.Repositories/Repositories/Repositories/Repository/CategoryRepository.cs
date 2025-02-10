using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class CategoryRepository(BSDbContext context) : BaseRepository<Category>(context), ICategoryRepository
    {
        public async Task<List<Category>> GetAll(string? name)
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(c => !c.IsDeleted);
            if (!string.IsNullOrEmpty(name))
            {
                queryable = queryable.Where(c => c.CategoryName.ToLower().Trim().Contains(name.ToLower().Trim()));
            }
            var categories = await queryable.ToListAsync();
            return categories;
        }
        public async Task<(List<Category>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
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
                    queryable = queryable.Where(c => c.CategoryName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var categories = await queryable.ToListAsync();

            return (categories, totalOrigin);
        }
        public async Task<(List<Category>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(c => c.CategoryName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var categories = await queryable.ToListAsync();

            return (categories, totalOrigin);
        }
        public async Task<Category?> GetByID(Guid? id)
        {
            var query = GetQueryable(c => c.Id == id);
            var category = await query.Include(c => c.BookCategories)
                                        .ThenInclude(bc => bc.Book)
                                  .SingleOrDefaultAsync();

            return category;
        }
    }
}
