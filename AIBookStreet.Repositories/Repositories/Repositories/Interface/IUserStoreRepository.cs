using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IUserStoreRepository : IBaseRepository<UserStore>
    {
        Task<List<UserStore?>> GetByUserId(Guid userId);
        Task<List<UserStore?>> GetByStoreId(Guid storeId);
        Task<UserStore?> GetByUserIdAndStoreId(Guid userId, Guid storeId);
    }
}
