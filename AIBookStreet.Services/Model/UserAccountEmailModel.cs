using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class UserAccountEmailModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string TemporaryPassword { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string LoginUrl { get; set; } = null!;
        public string? BaseImgUrl { get; set; }
        public Guid? RequestedRoleId { get; set; }
        public string? RequestedRoleName { get; set; }
    }
} 