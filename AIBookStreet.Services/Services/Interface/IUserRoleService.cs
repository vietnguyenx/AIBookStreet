using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserRoleService
    {
        Task<List<UserRoleModel>> GetAll();
        Task<List<UserRoleModel>?> GetByUserId(Guid userId);
        Task<List<UserRoleModel>?> GetByRoleId(Guid roleId);
        Task<bool> Add(UserRoleModel userRoleModel);
        Task<bool> Delete(Guid idUser, Guid idRole);
        Task<bool> ApproveRole(Guid userId, Guid roleId, bool approve);
        Task<List<UserRoleModel>?> GetPendingRoleRequests();
    }
}
