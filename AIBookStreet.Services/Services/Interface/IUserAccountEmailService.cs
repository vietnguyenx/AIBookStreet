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
        Task<bool> SendContractNotificationEmailAsync(ContractNotificationEmailModel contractInfo);
        Task<bool> SendEmailWithRetryAsync(Func<Task> emailSendAction, string recipientEmail, string emailType);
    }
} 