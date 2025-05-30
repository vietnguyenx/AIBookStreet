using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.Options;
using Razor.Templating.Core;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class UserAccountEmailService : IUserAccountEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly SmtpSettings _smtpSettings;
        private readonly IRazorTemplateEngine _razorTemplateEngine;
        private readonly ILogger<UserAccountEmailService> _logger;

        public UserAccountEmailService(
            SmtpClient smtpClient, 
            IOptions<SmtpSettings> smtpSettings, 
            IRazorTemplateEngine razorTemplateEngine,
            ILogger<UserAccountEmailService> logger)
        {
            _smtpClient = smtpClient;
            _smtpSettings = smtpSettings.Value;
            _razorTemplateEngine = razorTemplateEngine;
            _logger = logger;
        }

        public async Task<bool> SendAccountCreatedEmailAsync(UserAccountEmailModel userAccountInfo)
        {
            return await SendEmailWithRetryAsync(async () =>
            {
                _logger.LogInformation($"Bắt đầu gửi email thông báo tài khoản cho {userAccountInfo.Email}");

                // Tạo nội dung email từ template
                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/UserAccountCreated.cshtml", userAccountInfo);

                // Tạo email
                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(userAccountInfo.Email);

                var mail = new MailMessage(from, to)
                {
                    Subject = "[AIBookStreet] Tài khoản của bạn đã được tạo thành công",
                    IsBodyHtml = true
                };

                // Thêm nội dung HTML
                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                mail.AlternateViews.Add(view);

                // Gửi email
                await _smtpClient.SendMailAsync(mail);
                
                // Dispose mail object
                mail.Dispose();

                _logger.LogInformation($"Đã gửi email thông báo tài khoản thành công cho {userAccountInfo.Email}");
            }, userAccountInfo.Email, "Account Creation");
        }

        public async Task<bool> SendRoleApprovalEmailAsync(RoleApprovalEmailModel roleApprovalInfo)
        {
            return await SendEmailWithRetryAsync(async () =>
            {
                _logger.LogInformation($"Bắt đầu gửi email thông báo phê duyệt role cho {roleApprovalInfo.Email}");

                // Tạo nội dung email từ template
                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/RoleApproval.cshtml", roleApprovalInfo);

                // Tạo email
                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(roleApprovalInfo.Email);

                var subject = roleApprovalInfo.IsApproved 
                    ? $"[AIBookStreet] Yêu cầu quyền {roleApprovalInfo.RoleName} đã được phê duyệt"
                    : $"[AIBookStreet] Yêu cầu quyền {roleApprovalInfo.RoleName} đã bị từ chối";

                var mail = new MailMessage(from, to)
                {
                    Subject = subject,
                    IsBodyHtml = true
                };

                // Thêm nội dung HTML
                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                mail.AlternateViews.Add(view);

                // Gửi email
                await _smtpClient.SendMailAsync(mail);
                
                // Dispose mail object
                mail.Dispose();

                _logger.LogInformation($"Đã gửi email thông báo phê duyệt role thành công cho {roleApprovalInfo.Email}");
            }, roleApprovalInfo.Email, "Role Approval");
        }

        public async Task<bool> SendContractNotificationEmailAsync(ContractNotificationEmailModel contractInfo)
        {
            return await SendEmailWithRetryAsync(async () =>
            {
                _logger.LogInformation($"Bắt đầu gửi email thông báo hợp đồng thuê store cho {contractInfo.Email}");

                // Tạo nội dung email từ template
                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/ContractNotification.cshtml", contractInfo);

                // Tạo email
                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(contractInfo.Email);

                var mail = new MailMessage(from, to)
                {
                    Subject = "[AIBookStreet] Hợp đồng thuê cửa hàng đã được tạo thành công",
                    IsBodyHtml = true
                };

                // Thêm nội dung HTML
                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                mail.AlternateViews.Add(view);

                // Gửi email
                await _smtpClient.SendMailAsync(mail);
                
                // Dispose mail object
                mail.Dispose();

                _logger.LogInformation($"Đã gửi email thông báo hợp đồng thuê store thành công cho {contractInfo.Email}");
            }, contractInfo.Email, "Contract Notification");
        }

        public async Task<bool> SendContractExpirationEmailAsync(ContractExpirationEmailModel contractExpirationInfo)
        {
            return await SendEmailWithRetryAsync(async () =>
            {
                _logger.LogInformation($"Bắt đầu gửi email thông báo hợp đồng sắp hết hạn cho {contractExpirationInfo.Email}");

                // Tạo nội dung email từ template
                var htmlBody = await _razorTemplateEngine.RenderAsync("ResponseModel/ContractExpiration.cshtml", contractExpirationInfo);

                // Tạo email
                var from = new MailAddress(_smtpSettings.From);
                var to = new MailAddress(contractExpirationInfo.Email);

                var subject = contractExpirationInfo.DaysUntilExpiration == 1 
                    ? "[AIBookStreet] ⚠️ Hợp đồng thuê cửa hàng sẽ hết hạn vào ngày mai!"
                    : $"[AIBookStreet] ⚠️ Hợp đồng thuê cửa hàng sẽ hết hạn trong {contractExpirationInfo.DaysUntilExpiration} ngày";

                var mail = new MailMessage(from, to)
                {
                    Subject = subject,
                    IsBodyHtml = true
                };

                // Thêm nội dung HTML
                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                mail.AlternateViews.Add(view);

                // Gửi email
                await _smtpClient.SendMailAsync(mail);
                
                // Dispose mail object
                mail.Dispose();

                _logger.LogInformation($"Đã gửi email thông báo hợp đồng sắp hết hạn thành công cho {contractExpirationInfo.Email}");
            }, contractExpirationInfo.Email, "Contract Expiration");
        }

        public async Task<bool> SendEmailWithRetryAsync(Func<Task> emailSendAction, string recipientEmail, string emailType)
        {
            var maxRetries = _smtpSettings.MaxRetryAttempts;
            var retryDelay = _smtpSettings.RetryDelayMs;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await emailSendAction();
                    return true;
                }
                catch (SmtpException smtpEx)
                {
                    _logger.LogWarning($"SMTP Error (Attempt {attempt}/{maxRetries}) khi gửi {emailType} email cho {recipientEmail}: {smtpEx.Message}");
                    
                    if (attempt == maxRetries)
                    {
                        _logger.LogError($"Đã thử {maxRetries} lần nhưng không thể gửi {emailType} email cho {recipientEmail}. SMTP Error: {smtpEx.Message}");
                        return false;
                    }
                    
                    await Task.Delay(retryDelay * attempt); // Exponential backoff
                }
                catch (InvalidOperationException invOpEx)
                {
                    _logger.LogError($"Invalid Operation khi gửi {emailType} email cho {recipientEmail}: {invOpEx.Message}");
                    return false; // Don't retry for configuration errors
                }
                catch (ArgumentException argEx)
                {
                    _logger.LogError($"Argument Error khi gửi {emailType} email cho {recipientEmail}: {argEx.Message}");
                    return false; // Don't retry for argument errors
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"General Error (Attempt {attempt}/{maxRetries}) khi gửi {emailType} email cho {recipientEmail}: {ex.Message}");
                    
                    if (attempt == maxRetries)
                    {
                        _logger.LogError($"Đã thử {maxRetries} lần nhưng không thể gửi {emailType} email cho {recipientEmail}. Error: {ex.Message}");
                        return false;
                    }
                    
                    await Task.Delay(retryDelay * attempt); // Exponential backoff
                }
            }

            return false;
        }
    }
} 