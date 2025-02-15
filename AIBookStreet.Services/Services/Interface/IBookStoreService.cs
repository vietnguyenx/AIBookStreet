using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IBookStoreService
    {
        Task<List<BookStoreModel>> GetAll();
        Task<List<BookStoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<BookStoreModel?> GetById(Guid id);
        Task<(List<BookStoreModel>?, long)> SearchPagination(BookStoreModel bookStoreModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<BookStoreModel>?> SearchWithoutPagination(BookStoreModel bookStoreModel);
        Task<bool> Add(BookStoreModel bookStoreModel);
        Task<bool> Update(BookStoreModel bookStoreModel);
        Task<bool> Delete(Guid id);
        Task<long> GetTotalCount();
    }
}
