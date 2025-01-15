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
    public class BookStoreRepository : BaseRepository<BookStore>, IBookStoreRepository
    {
        private readonly BSDbContext _context;

        public BookStoreRepository(BSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookStore>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(bs => bs.Images)
                .ToListAsync();
        }

        public async Task<BookStore?> GetById(Guid id)
        {
            var query = GetQueryable(bs => bs.Id == id);
            var bookStore = await query
                .Include(bs => bs.Images)
                .Include(bs => bs.Inventories)
                .SingleOrDefaultAsync();

            return bookStore;
        }

        public async Task<(List<BookStore>, long)> Search(BookStore bookStore, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable();
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(bookStore.BookStoreName))
                {
                    queryable = queryable.Where(bs => bs.BookStoreName.ToLower().Trim().Contains(bookStore.BookStoreName.ToLower().Trim()));
                }

                if (!string.IsNullOrEmpty(bookStore.Email))
                {
                    queryable = queryable.Where(bs => bs.Email.ToLower().Trim().Contains(bookStore.Email.ToLower().Trim()));
                }

            }
            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var bookstores = await queryable
                .Include(bs => bs.Images)
                .ToListAsync();

            return (bookstores, totalOrigin);
        }
    }
}
