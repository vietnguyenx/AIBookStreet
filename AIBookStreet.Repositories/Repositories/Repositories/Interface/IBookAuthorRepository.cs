using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IBookAuthorRepository : IBaseRepository<BookAuthor>
    {
        Task<List<BookAuthor>> GetAll();
        Task<(List<BookAuthor> , long)> GetAllPagination(string? key, Guid? bookID, Guid? authorID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<BookAuthor?> GetByID(Guid id);
        Task<List<BookAuthor>?> GetByElement(Guid? bookID, Guid? authorID);
    }
}
