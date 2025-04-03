using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserStoreService
    {
        Task<List<UserStoreModel>> GetAll();
        Task<List<UserStoreModel>?> GetByUserId(Guid userId);
        Task<List<UserStoreModel>?> GetByStoreId(Guid storeId);
        Task<bool> Add(UserStoreModel userStoreModel);
        Task<bool> Delete(Guid idUser, Guid idStore);
    }
}
