﻿using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IBookAuthorService
    {
        Task<(BookAuthor?, long)> AddABookAuthor(BookAuthorModel model);
        Task<(long, BookAuthor?)> UpdateABookAuthor(BookAuthorModel bookAuthorModel);
        Task<(long, BookAuthor?)> DeleteABookAuthor(BookAuthorModel bookAuthorModel);
        Task<BookAuthor?> GetABookAuthorById(Guid id);
        Task<List<BookAuthor>?> GetBookAuthorByElement(Guid? bookID, Guid? authorID);
        Task<List<BookAuthor>?> GetAllActiveBookAuthors();
        Task<(List<BookAuthor>?, long)> GetAllBookAuthorsPagination(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
