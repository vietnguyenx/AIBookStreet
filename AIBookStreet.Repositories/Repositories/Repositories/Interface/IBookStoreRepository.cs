using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IBookStoreRepository : IBaseRepository<BookStore>
    {
        Task<List<BookStore>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<BookStore?> GetById(Guid id);
        Task<(List<BookStore>, long)> SearchPagination(BookStore bookStore, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<BookStore>> SearchWithoutPagination(BookStore bookStore);
    }
}
