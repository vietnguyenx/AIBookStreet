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
        Task<List<Inventory?>> GetByBookId(Guid entityId);
        Task<List<Inventory?>> GetByStoreId(Guid storeId);
        Task<Inventory?> GetByBookIdAndStoreId(Guid? entityId, Guid storeId);
    }
}
