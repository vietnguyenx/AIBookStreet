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
        Task<List<InventoryModel>?> GetByBookId(Guid bookId);
        Task<List<InventoryModel>?> GetByStoreId(Guid storeId);
        Task<bool> Add(InventoryModel inventoryModel);
        Task<bool> Delete(Guid bookId, Guid storeId);
        Task<(bool, string)> UpdateQuantityByISBN(string ISBN, Guid storeId, int quantity);
    }
}
