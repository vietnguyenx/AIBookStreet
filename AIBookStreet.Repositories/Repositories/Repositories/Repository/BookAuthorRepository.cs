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
    public class BookAuthorRepository(BSDbContext context) : BaseRepository<BookAuthor>(context), IBookAuthorRepository
    {
        public async Task<List<BookAuthor>> GetAll()
        {
            var queryable = GetQueryable();
            queryable = queryable.Where(ba => !ba.IsDeleted);
            var bookAuthors = await queryable.Include(ba => ba.Book).Include(ba => ba.Author).ToListAsync();
            return bookAuthors;
        }
        public async Task<(List<BookAuthor>, long)> GetAllPagination(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);
            queryable = queryable.Where(ba => !ba.IsDeleted);
            queryable = queryable.Where(ba => !ba.Book.IsDeleted && !ba.Author.IsDeleted);

            queryable = queryable.Include(ba => ba.Book).Include(ba => ba.Author);
            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ba => ba.Author.AuthorName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(ba.Book.Title) && ba.Book.Title.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (bookID != null)
                {
                    queryable = queryable.Where(ba => ba.BookId.Equals(bookID));
                }
                if (authorID != null)
                {
                    queryable = queryable.Where(ba => ba.AuthorId.Equals(authorID));
                }
            }

            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;
            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var bookAuthors = await queryable.ToListAsync();

            return (bookAuthors, totalOrigin);
        }
        public async Task<(List<BookAuthor>, long)> GetAllPaginationForAdmin(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            var queryable = GetQueryable();
            string field = string.IsNullOrEmpty(sortField) ? "CreatedDate" : sortField;
            var order = desc != null && (desc != false);
            queryable = order ? base.ApplySort(queryable, field, 0) : base.ApplySort(queryable, field, 1);

            queryable = queryable.Include(ba => ba.Book).Include(ba => ba.Author);
            if (queryable.Any())
            {
                if (!string.IsNullOrEmpty(key))
                {
                    queryable = queryable.Where(ba => ba.Author.AuthorName.ToLower().Trim().Contains(key.ToLower().Trim())
                                                   || (!string.IsNullOrEmpty(ba.Book.Title) && ba.Book.Title.ToLower().Trim().Contains(key.ToLower().Trim())));
                }
                if (bookID != null)
                {
                    queryable = queryable.Where(ba => ba.BookId.Equals(bookID));
                }
                if (authorID != null)
                {
                    queryable = queryable.Where(ba => ba.AuthorId.Equals(authorID));
                }
            }

            var totalOrigin = queryable.Count();

            pageNumber = pageNumber == null ? 1 : pageNumber;
            pageSize = pageSize == null ? 10 : pageSize;
            queryable = GetQueryablePagination(queryable, (int)pageNumber, (int)pageSize);

            var bookAuthors = await queryable.ToListAsync();

            return (bookAuthors, totalOrigin);
        }
        public async Task<BookAuthor?> GetByID(Guid id)
        {
            var query = GetQueryable(ba => ba.Id == id);
            var bookAuthor = await query.Include(ba => ba.Book)
                                  .Include(ba => ba.Author)
                                  .SingleOrDefaultAsync();

            return bookAuthor;
        }
        public async Task<List<BookAuthor>?> GetByElement(Guid? bookID, Guid? authorID)
        {
            var query = GetQueryable();
            if (bookID != null && authorID != null)
            {
                query = query.Where(ba => ba.BookId == bookID && ba.AuthorId == authorID);
            } else if (bookID != null && authorID == null)
            {
                query = query.Where(ba => ba.BookId == bookID);
            } else if (authorID != null && bookID == null)
            {
                query = query.Where(ba => ba.AuthorId == authorID);
            }

            var bookAuthors = await query.Include(ba => ba.Book)
                                  .Include(ba => ba.Author).ToListAsync();

            return bookAuthors;
        }
    }
}
