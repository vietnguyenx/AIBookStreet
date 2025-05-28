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
            try
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

                _logger.LogInformation($"Đã gửi email thông báo tài khoản thành công cho {userAccountInfo.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gửi email thông báo tài khoản cho {userAccountInfo.Email}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendRoleApprovalEmailAsync(RoleApprovalEmailModel roleApprovalInfo)
        {
            try
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

                _logger.LogInformation($"Đã gửi email thông báo phê duyệt role thành công cho {roleApprovalInfo.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gửi email thông báo phê duyệt role cho {roleApprovalInfo.Email}: {ex.Message}");
                return false;
            }
        }
    }
} 