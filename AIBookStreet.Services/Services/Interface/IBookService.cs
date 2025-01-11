using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IBookService
    {
        Task<List<BookModel>> GetAll();
        public Task<List<BookModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<BookModel?> GetById(Guid id);
        public Task<(List<BookModel>?, long)> Search(BookModel bookModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<bool> Add(BookModel BookModel);
        Task<bool> Update(BookModel BookModel);
        Task<bool> Delete(Guid id);
        public Task<long> GetTotalCount();
    }
}
