using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IStoreService
    {
        Task<List<StoreModel>> GetAll();
        Task<List<StoreModel>?> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<StoreModel?> GetById(Guid id);
        Task<(List<StoreModel>?, long)> SearchPagination(StoreModel bookStoreModel, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<StoreModel>?> SearchWithoutPagination(StoreModel bookStoreModel);
        Task<bool> Add(StoreModel bookStoreModel);
        Task<bool> Update(StoreModel bookStoreModel);
        Task<bool> Delete(Guid id);
        Task<long> GetTotalCount();
    }
}
