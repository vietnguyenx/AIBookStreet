using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IStoreRepository : IBaseRepository<Store>
    {
        Task<List<Store>> GetAllPagination(int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<Store?> GetById(Guid id);
        Task<(List<Store>, long)> SearchPagination(Store store, int pageNumber, int pageSize, string sortField, int sortOrder);
        Task<List<Store>> SearchWithoutPagination(Store store);
        Task<long> GetTotalCount();
        Task<List<Store>> GetStoresByMonth(int month, int year);
    }
}
