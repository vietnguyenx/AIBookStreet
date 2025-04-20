using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IInventoryService
    {
        Task<List<InventoryModel>> GetAll();
        Task<List<InventoryModel>?> GetByEntityId(Guid entityId);
        Task<List<InventoryModel>?> GetByStoreId(Guid storeId);
        Task<List<InventoryModel>?> GetBooksByStoreId(Guid storeId);
        Task<List<InventoryModel>?> GetSouvenirsByStoreId(Guid storeId);
        Task<(bool, string)> Add(InventoryModel inventoryModel);
        Task<(bool, string)> Update(Guid entityId, Guid storeId, int quantity);
        Task<bool> Delete(Guid entityId, Guid storeId);
        Task<(bool, string)> UpdateQuantityByISBN(string ISBN, Guid storeId, int quantity);
    }
}
