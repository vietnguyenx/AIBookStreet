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
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        private readonly BSDbContext _context;

        public BookRepository(BSDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            // loc theo trang
            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(b => b.Images)
                .Include(b => b.Publisher.PublisherName)
                .Include(b => b.BookCategories)
                .Include(b => b.BookAuthors )
                .Include(b => b.Inventories)
                .ToListAsync();
        }

        public async Task<Book?> GetById(Guid id)
        {
            var query = GetQueryable(m => m.Id == id);
            var book = await query
                .Include(b => b.Images)
                .Include(b => b.Publisher.PublisherName)
                .Include(b => b.BookCategories)
                .Include(b => b.BookAuthors)
                .Include(b => b.Inventories)
                .SingleOrDefaultAsync();

            return book;
        }

        public async Task<(List<Book>, long)> Search(Book Book, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(Book.Title))
                {
                    queryable = queryable.Where(b => b.Title.ToLower().Trim().Contains(Book.Title.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(Book.Code))
                {
                    queryable = queryable.Where(b => b.Code.ToLower().Trim().Contains(Book.Code.ToLower().Trim()));
                }

            }
            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var books = await queryable
                .Include(b => b.Images)
                .Include(b => b.Publisher.PublisherName)
                .Include(b => b.BookCategories)
                .Include(b => b.BookAuthors)
                .Include(b => b.Inventories)
                .ToListAsync();

            return (books, totalOrigin);
        }
        
    }
}
