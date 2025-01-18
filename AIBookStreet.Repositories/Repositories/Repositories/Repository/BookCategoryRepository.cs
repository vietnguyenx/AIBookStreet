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
    public class BookCategoryRepository(BSDbContext context) : BaseRepository<BookCategory>(context), IBookCategoryRepository
    {
        public async Task<List<BookCategory>> GetAll()
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(bc => !bc.IsDeleted);
            var bookCategories = await queryable.Include(bc => bc.Book).Include(bc => bc.Category).ToListAsync();
            return bookCategories;
        }
        public async Task<(List<BookCategory>, long)> GetAllPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            queryable = queryable.Include(bc => bc.Book).Include(bc => bc.Category);
            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(bc => bc.Category.CategoryName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(bc.Book.Title) && bc.Book.Title.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
            }

            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;
            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var bookCategories = await queryable.ToListAsync();

            return (bookCategories, totalOrigin);
        }
        public async Task<BookCategory?> GetByID(Guid id)
        {
            var query = GetQueryable(bc => bc.Id == id);
            var bookCategory = await query.Include(bc => bc.Book)
                                  .Include(bc => bc.Category)
                                  .SingleOrDefaultAsync();

            return bookCategory;
        }
        public async Task<List<BookCategory>?> GetByElement(Guid? bookID, Guid? categoryID)
        {
            var query = GetQueryable();
            if (bookID != null && categoryID != null)
            {
                query = query.Where(bc => bc.BookId == bookID && bc.CategoryId == categoryID);
            }
            else if (bookID != null && categoryID == null)
            {
                query = query.Where(bc => bc.BookId == bookID);
            }
            else if (categoryID != null && bookID == null)
            {
                query = query.Where(bc => bc.CategoryId == categoryID);
            }

            var bookCategories = await query.Include(bc => bc.Book)
                                  .Include(bc => bc.Category).ToListAsync();

            return bookCategories;
        }
    }
}
