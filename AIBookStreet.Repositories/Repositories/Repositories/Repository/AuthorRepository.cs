using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class AuthorRepository(BSDbContext context) : BaseRepository<Author>(context), IAuthorRepository
    {
        public async Task<List<Author>> GetAll()
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(at => !at.IsDeleted);
            var authors = await queryable.ToListAsync();
            return authors;
        }
        public async Task<(List<Author>, long)> GetAllPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(at => !at.IsDeleted);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(at => at.AuthorName.ToLower().Trim().Contains(key.ToLower().Trim()) 
                                                   || (!string.IsNullOrEmpty(at.Nationality) && at.Nationality.ToLower().Trim().Contains(key.ToLower().Trim()))
                                                   || (!string.IsNullOrEmpty(at.Biography) && at.Biography.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var authors = await queryable
                .Include(at => at.Images).ToListAsync();

            return (authors, totalOrigin);
        }
        public async Task<(List<Author>, long)> GetAllPaginationForAdmin(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(at => at.AuthorName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(at.Nationality) && at.Nationality.ToLower().Trim().Contains(key.ToLower().Trim()))
                                                   || (!string.IsNullOrEmpty(at.Biography) && at.Biography.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }
            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;

            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var authors = await queryable
                .Include(at => at.Images).ToListAsync();

            return (authors, totalOrigin);
        }
        public async Task<Author?> GetByID(Guid id)
        {
            var query = GetQueryable(at => at.Id == id);
            var author = await query.Include(at => at.BookAuthors)
                                    .ThenInclude(ba => ba.Book)
                                  .Include(at => at.Images)
                                  .SingleOrDefaultAsync();

            return author;
        }
    }
}
