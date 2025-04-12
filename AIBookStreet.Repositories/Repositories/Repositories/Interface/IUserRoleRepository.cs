using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Repositories.Repositories.Interface
{
    public interface IUserRoleRepository : IBaseRepository<UserRole>
    {
        Task<List<UserRole?>> GetByUserId(Guid userId);
        Task<List<UserRole?>> GetByRoleId(Guid roleId);
        Task<UserRole?> GetByUserIdAndRoleId(Guid userId, Guid roleId);
        Task<UserRole?> FindDeletedUserRole(Guid userId, Guid roleId);
    }
}
