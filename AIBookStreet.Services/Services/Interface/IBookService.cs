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
        Task<List<BookModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<BookModel?> GetById(Guid id);
        Task<(List<BookModel>?, long)> SearchPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<BookModel>?> SearchWithoutPagination(BookModel bookModel, DateTime? startDate, DateTime? endDate);

        Task<bool> Add(BookModel bookModel);
        Task<bool> Update(BookModel bookModel);
        Task<bool> Delete(Guid id);
        Task<long> GetTotalCount();
    }
}
