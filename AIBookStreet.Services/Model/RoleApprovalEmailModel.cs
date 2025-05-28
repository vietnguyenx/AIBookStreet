using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class RoleApprovalEmailModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsApproved { get; set; }
        public DateTime DecisionDate { get; set; }
        public string LoginUrl { get; set; } = null!;
        public string? BaseImgUrl { get; set; }
    }
} 