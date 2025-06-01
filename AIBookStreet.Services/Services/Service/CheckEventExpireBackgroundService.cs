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
    public class CheckEventExpireBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<CheckEventExpireBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<CheckEventExpireBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {          
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    DateTime midnight = now.Date.AddDays(1);
                    var delay = midnight - now;
                    await Task.Delay(delay, stoppingToken);
                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailStatistcService = scope.ServiceProvider.GetRequiredService<IEventRegistrationService>();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                    var expiredEvents = await eventService.GetExpireEvents();
                    if (expiredEvents != null)
                    {
                        foreach (var expiredEvent in expiredEvents)
                        {
                            await eventService.OpenState(expiredEvent.Id);
                            var model = new Model.ExportStatisticModel
                            {
                                Email = expiredEvent.OrganizerEmail,
                                EventId = expiredEvent.Id,
                            };
                            await emailStatistcService.ExportStatisticReport(model);
                            _logger.LogInformation($"Đã gửi số liệu đến {model.Email} thành công cho EventId: {model.EventId}");
                        }
                    }                 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Check Event Expired Background Service: {Message}", ex.Message);

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Check Event Expired Background Service đã dừng.");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Check Event Expired Background Service đang dừng...");
            await base.StopAsync(stoppingToken);
        }
    }
}
