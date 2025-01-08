using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class UserRole : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
