using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class EmailSenderBackgroundService(IEmailQueueService emailQueue, IServiceScopeFactory serviceScopeFactory, ILogger<EmailSenderBackgroundService> logger) : BackgroundService
    {
        private readonly IEmailQueueService _emailQueue = emailQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<EmailSenderBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Sender Background Service đang chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_emailQueue.TryDequeue(out var message))
                {
                    _logger.LogInformation($"Đang xử lý email cho RegistrationId: {message?.RegistrationId}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEventRegistrationService>(); // Thay YourNamespace.YourEmailSendingService bằng service chứa phương thức SendEmail của bạn
                    try
                    {
                        await emailService.SendEmai(message);
                        _logger.LogInformation($"Đã gửi email thành công cho RegistrationId: {message?.RegistrationId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khi gửi email cho RegistrationId: {message?.RegistrationId}: {ex.Message}");
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Email Sender Background Service đã dừng.");
        }
    }
}
