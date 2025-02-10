using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IAuthorService
    {
        Task<Author?> AddAnAuthor(AuthorModel authorModel);
        Task<(long, Author?)> UpdateAnAuthor(Guid? authorID, AuthorModel authorModel);
        Task<(long, Author?)> DeleteAnAuthor(Guid id);
        Task<Author?> GetAnAuthorById(Guid id);
        Task<List<Author>?> GetAllActiveAuthors(string? authorName);
        Task<(List<Author>?, long)> GetAllAuthorsPagination(string? key, int? pageNumber, int? pageSize, string? sortField, bool? desc);
    }
}
