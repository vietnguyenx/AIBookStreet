using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IInventoryRepository : IBaseRepository<Inventory>
    {
        Task<List<Inventory?>> GetByEntityId(Guid entityId);
        Task<List<Inventory?>> GetByStoreId(Guid storeId);
        Task<Inventory?> GetByEntityIdAndStoreId(Guid? entityId, Guid storeId);
        Task<Inventory?> GetByID(Guid? storeId);
        Task<List<Inventory?>> GetBooksByStoreId(Guid storeId);
        Task<List<Inventory?>> GetSouvenirsByStoreId(Guid storeId);
    }
}
