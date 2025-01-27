using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IBookCategoryRepository : IBaseRepository<BookCategory>
    {
        Task<List<BookCategory>> GetAll();
        Task<(List<BookCategory>, long)> GetAllPagination(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<(List<BookCategory>, long)> GetAllPaginationForAdmin(string? key, Guid? bookID, Guid? categoryID, int? pageNumber, int? pageSize, string? sortField, bool? desc);
        Task<BookCategory?> GetByID(Guid id);
        Task<List<BookCategory>?> GetByElement(Guid? bookID, Guid? categoryID);
    }
}
