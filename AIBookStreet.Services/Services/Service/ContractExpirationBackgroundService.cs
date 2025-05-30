using AIBookStreet.Services.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Service
{
    public class ContractExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ContractExpirationBackgroundService> _logger;
        private readonly TimeSpan _dailyRunTime = new TimeSpan(9, 0, 0); // Chạy lúc 9:00 AM

        public ContractExpirationBackgroundService(
            IServiceScopeFactory serviceScopeFactory, 
            ILogger<ContractExpirationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Contract Expiration Background Service đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextRunTime(now);
                    var delay = nextRun - now;

                    _logger.LogInformation($"Lần chạy tiếp theo sẽ là: {nextRun:yyyy-MM-dd HH:mm:ss} (sau {delay.TotalHours:F1} giờ)");

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ProcessContractExpirations();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Contract Expiration Background Service đã bị hủy.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Contract Expiration Background Service: {Message}", ex.Message);
                    
                    // Nếu có lỗi, đợi 1 giờ trước khi thử lại
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Contract Expiration Background Service đã dừng.");
        }

        private DateTime GetNextRunTime(DateTime now)
        {
            var today = now.Date;
            var todayRunTime = today + _dailyRunTime;

            // Nếu chưa qua giờ chạy hôm nay, chạy hôm nay
            if (now < todayRunTime)
            {
                return todayRunTime;
            }
            
            // Nếu đã qua giờ chạy hôm nay, chạy vào ngày mai
            return todayRunTime.AddDays(1);
        }

        private async Task ProcessContractExpirations()
        {
            try
            {
                _logger.LogInformation("Bắt đầu kiểm tra hợp đồng sắp hết hạn...");

                using var scope = _serviceScopeFactory.CreateScope();
                var userStoreService = scope.ServiceProvider.GetRequiredService<IUserStoreService>();

                // Gửi email thông báo hết hạn
                var (totalSent, message) = await userStoreService.SendExpirationWarningEmails();
                
                _logger.LogInformation($"✅ Hoàn thành kiểm tra hợp đồng sắp hết hạn: {message}");

                // Cập nhật các hợp đồng đã hết hạn
                var updateResult = await userStoreService.UpdateExpiredContracts();
                if (updateResult)
                {
                    _logger.LogInformation("✅ Đã cập nhật trạng thái các hợp đồng hết hạn");
                }
                else
                {
                    _logger.LogWarning("⚠️ Có lỗi khi cập nhật trạng thái hợp đồng hết hạn");
                }

                _logger.LogInformation($"📊 Tổng kết: Đã gửi {totalSent} email thông báo hết hạn hợp đồng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Lỗi khi xử lý hợp đồng sắp hết hạn: {Message}", ex.Message);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Contract Expiration Background Service đang dừng...");
            await base.StopAsync(stoppingToken);
        }
    }
} 