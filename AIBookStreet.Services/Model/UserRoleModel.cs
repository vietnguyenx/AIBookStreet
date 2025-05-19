using AIBookStreet.Repositories.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class UserRoleModel : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public bool IsApproved { get; set; } = false;

        public UserModel User { get; set; }
        public RoleModel Role { get; set; }
    }
}
