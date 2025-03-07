using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? BaseImgUrl { get; set; }

        public virtual Store? Store { get; set; }
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<Image>? Images { get; set; }
    }
}
