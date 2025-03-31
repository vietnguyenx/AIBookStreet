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

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            return await queryable
                .Include(b => b.Images)
                .ToListAsync();
        }

        public async Task<Book?> GetById(Guid id)
        {
            var query = GetQueryable(b => b.Id == id);
            var book = await query
                .Include(b => b.Images)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                .Include(b => b.BookAuthors)
                .Include(b => b.Inventories)
                .SingleOrDefaultAsync();

            return book;
        }

        public async Task<(List<Book>, long)> SearchPagination(Book book, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            var queryable = GetQueryable()
                .Where(b => !b.IsDeleted);
            queryable = base.ApplySort(queryable, sortField, sortOrder);

            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(book.Code))
                {
                    queryable = queryable.Where(b =>
                        EF.Functions.Collate(b.Code, "Latin1_General_CI_AI").Contains(book.Code));
                }

                if (!string.IsNullOrEmpty(book.Title))
                {
                    queryable = queryable.Where(b =>
                        EF.Functions.Collate(b.Title, "Latin1_General_CI_AI").Contains(book.Title));
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    queryable = queryable.Where(b => b.PublicationDate >= startDate.Value && b.PublicationDate <= endDate.Value);
                }
                else if (startDate.HasValue)
                {
                    queryable = queryable.Where(b => b.PublicationDate >= startDate.Value);
                }
                else if (endDate.HasValue)
                {
                    queryable = queryable.Where(b => b.PublicationDate <= endDate.Value);
                }

                // Handle price range search
                if (minPrice.HasValue && maxPrice.HasValue)
                {
                    // Search between min and max price
                    queryable = queryable.Where(m => m.Price >= minPrice && m.Price <= maxPrice);
                }
                else if (minPrice.HasValue)
                {
                    // Search for price >= minPrice
                    queryable = queryable.Where(m => m.Price >= minPrice);
                }
                else if (maxPrice.HasValue)
                {
                    // Search for price <= maxPrice
                    queryable = queryable.Where(m => m.Price <= maxPrice);
                }

                if (!string.IsNullOrEmpty(book.Languages))
                {
                    queryable = queryable.Where(b =>
                        EF.Functions.Collate(b.Languages, "Latin1_General_CI_AI").Contains(book.Languages));
                }

                if (!string.IsNullOrEmpty(book.Size))
                {
                    queryable = queryable.Where(b =>
                        EF.Functions.Collate(b.Size, "Latin1_General_CI_AI").Contains(book.Size));
                }

                if (!string.IsNullOrEmpty(book.Status))
                {
                    queryable = queryable.Where(b =>
                        EF.Functions.Collate(b.Status, "Latin1_General_CI_AI").Contains(book.Status));
                }

                if (book.BookCategories != null && book.BookCategories.Any())
                {
                    var categoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList();
                    queryable = queryable.Where(b => b.BookCategories.Any(bc => categoryIds.Contains(bc.CategoryId)));
                }
            }
            var totalOrigin = queryable.Count();

            queryable = GetQueryablePagination(queryable, pageNumber, pageSize);

            var books = await queryable
                .Include(b => b.Images)
                .Include(b => b.BookCategories)
                .ToListAsync();

            return (books, totalOrigin);
        }

        public async Task<List<Book>> SearchWithoutPagination(Book book, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice)
        {
            var queryable = GetQueryable()
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(book.Code))
            {
                queryable = queryable.Where(b =>
                    EF.Functions.Collate(b.Code, "Latin1_General_CI_AI").Contains(book.Code));
            }

            if (!string.IsNullOrEmpty(book.Title))
            {
                queryable = queryable.Where(b =>
                    EF.Functions.Collate(b.Title, "Latin1_General_CI_AI").Contains(book.Title));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                queryable = queryable.Where(b => b.PublicationDate >= startDate.Value && b.PublicationDate <= endDate.Value);
            }
            else if (startDate.HasValue)
            {
                queryable = queryable.Where(b => b.PublicationDate >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                queryable = queryable.Where(b => b.PublicationDate <= endDate.Value);
            }

            // Handle price range search
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                // Search between min and max price
                queryable = queryable.Where(m => m.Price >= minPrice && m.Price <= maxPrice);
            }
            else if (minPrice.HasValue)
            {
                // Search for price >= minPrice
                queryable = queryable.Where(m => m.Price >= minPrice);
            }
            else if (maxPrice.HasValue)
            {
                // Search for price <= maxPrice
                queryable = queryable.Where(m => m.Price <= maxPrice);
            }

            if (!string.IsNullOrEmpty(book.Languages))
            {
                queryable = queryable.Where(b =>
                    EF.Functions.Collate(b.Languages, "Latin1_General_CI_AI").Contains(book.Languages));
            }

            if (!string.IsNullOrEmpty(book.Size))
            {
                queryable = queryable.Where(b =>
                    EF.Functions.Collate(b.Size, "Latin1_General_CI_AI").Contains(book.Size));
            }

            if (!string.IsNullOrEmpty(book.Status))
            {
                queryable = queryable.Where(b =>
                    EF.Functions.Collate(b.Status, "Latin1_General_CI_AI").Contains(book.Status));
            }

            return await queryable.Include(b => b.Images).ToListAsync();
        }
    }
}
