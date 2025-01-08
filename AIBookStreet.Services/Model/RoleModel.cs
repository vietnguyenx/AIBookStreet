using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class RoleModel : BaseModel
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }

        public IList<UserRoleModel> UserRoles { get; set; }
    }
}
