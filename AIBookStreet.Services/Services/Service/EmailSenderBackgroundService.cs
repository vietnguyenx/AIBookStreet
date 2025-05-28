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
    public class EmailSenderBackgroundService(IEventRegistrationQueueService eventRegistrationQueue, IExportEventStatisticQueueService exportEventStatisticQueue, IServiceScopeFactory serviceScopeFactory, ILogger<EmailSenderBackgroundService> logger) : BackgroundService
    {
        private readonly IEventRegistrationQueueService _eventRegistrationQueue = eventRegistrationQueue;
        private readonly IExportEventStatisticQueueService _exportEventStatisticQueue = exportEventStatisticQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<EmailSenderBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Sender Background Service đang chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_eventRegistrationQueue.TryDequeue(out var message))
                {
                    _logger.LogInformation($"Đang xử lý email cho RegistrationId: {message}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailRegistrationService = scope.ServiceProvider.GetRequiredService<IEventRegistrationService>();
                    var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
                    try
                    {
                        var ticket = await ticketService.GetTicketById(message);
                        await emailRegistrationService.SendRegistrationEmai(ticket);
                        _logger.LogInformation($"Đã gửi email thành công cho RegistrationId: {message}");
                    }
                    catch
                    {
                        _logger.LogError($"Lỗi khi gửi email cho RegistrationId: {message}");
                    }
                } else if (_exportEventStatisticQueue.TryDequeue(out var model)){
                    _logger.LogInformation($"Đang gửi số liệu đến {model.Email} cho EventId: {model.EventId}");
                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailStatistcService = scope.ServiceProvider.GetRequiredService<IEventRegistrationService>(); 
                    try
                    {
                        await emailStatistcService.ExportStatisticReport(model);
                        _logger.LogInformation($"Đã gửi số liệu đến {model.Email} thành công cho EventId: {model.EventId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Lỗi khi gửi số liệu cho EventId: {model.EventId}: {ex.Message}");
                    }
                }
                else
                {
                    await Task.Delay(10000, stoppingToken);
                }
            }

            _logger.LogInformation("Email Sender Background Service đã dừng.");
        }
    }
}
