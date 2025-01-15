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
        public Task<List<BookStoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<BookStoreModel?> GetById(Guid id);
        public Task<(List<BookStoreModel>?, long)> Search(BookStoreModel bookStoreModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<bool> Add(BookStoreModel bookStoreModel);
        Task<bool> Update(BookStoreModel bookStoreModel);
        Task<bool> Delete(Guid id);
        public Task<long> GetTotalCount();
    }
}
