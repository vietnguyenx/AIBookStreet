using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IRoleService
    { 
        Task<List<RoleModel>> GetAll();
        Task<RoleModel?> GetById(Guid id);
        Task<bool> Add(RoleModel roleModel);
        Task<bool> Delete(Guid roleId);
    }
}
