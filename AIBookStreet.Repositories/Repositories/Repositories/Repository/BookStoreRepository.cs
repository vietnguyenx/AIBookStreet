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
                .Include(bs => bs.Manager)
                .Include(bs => bs.Zone)
                .SingleOrDefaultAsync();

            return bookStore;
        }

        public async Task<(List<BookStore>, long)> SearchPagination(BookStore bookStore, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable()
                .Where(bs => !bs.IsDeleted);
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(bookStore.BookStoreName))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.BookStoreName, "Latin1_General_CI_AI").Contains(bookStore.BookStoreName));
                }

                if (!string.IsNullOrEmpty(bookStore.Address))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Address, "Latin1_General_CI_AI").Contains(bookStore.Address));
                }

                if (!string.IsNullOrEmpty(bookStore.Phone))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Phone, "Latin1_General_CI_AI").Contains(bookStore.Phone));
                }

                if (!string.IsNullOrEmpty(bookStore.Email))
                {
                    queryable = queryable.Where(bs =>
                        EF.Functions.Collate(bs.Email, "Latin1_General_CI_AI").Contains(bookStore.Email));
                }

                if (bookStore.OpeningTime.HasValue)
                {
                    queryable = queryable.Where(bs => bs.OpeningTime.Value.Date == bookStore.OpeningTime.Value.Date);
                }

                if (bookStore.ClosingTime.HasValue)
                {
                    queryable = queryable.Where(bs => bs.ClosingTime.Value.Date == bookStore.ClosingTime.Value.Date);
                }
            }
            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var bookstores = await queryable
                .Include(bs => bs.Images)
                .ToListAsync();

            return (bookstores, totalOrigin);
        }

        public async Task<List<BookStore>> SearchWithoutPagination(BookStore bookStore)
        {
            var queryable = GetQueryable()
                .Where(bs => !bs.IsDeleted);

            if (!string.IsNullOrEmpty(bookStore.BookStoreName))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.BookStoreName, "Latin1_General_CI_AI").Contains(bookStore.BookStoreName));
            }

            if (!string.IsNullOrEmpty(bookStore.Address))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Address, "Latin1_General_CI_AI").Contains(bookStore.Address));
            }

            if (!string.IsNullOrEmpty(bookStore.Phone))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Phone, "Latin1_General_CI_AI").Contains(bookStore.Phone));
            }

            if (!string.IsNullOrEmpty(bookStore.Email))
            {
                queryable = queryable.Where(bs =>
                    EF.Functions.Collate(bs.Email, "Latin1_General_CI_AI").Contains(bookStore.Email));
            }

            if (bookStore.OpeningTime.HasValue)
            {
                queryable = queryable.Where(bs => bs.OpeningTime.Value.Date == bookStore.OpeningTime.Value.Date);
            }

            if (bookStore.ClosingTime.HasValue)
            {
                queryable = queryable.Where(bs => bs.ClosingTime.Value.Date == bookStore.ClosingTime.Value.Date);
            }

            return await queryable.Include(bs => bs.Images).ToListAsync();
        }
    }
}
