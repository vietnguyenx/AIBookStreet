using AIBookStreet.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IUserAccountEmailService
    {
        Task<bool> SendAccountCreatedEmailAsync(UserAccountEmailModel userAccountInfo);
        Task<bool> SendRoleApprovalEmailAsync(RoleApprovalEmailModel roleApprovalInfo);
    }
} 