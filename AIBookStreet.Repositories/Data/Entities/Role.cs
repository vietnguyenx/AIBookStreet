using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
