using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services
{
    public class PaymentTimeoutWorker(
        IServiceProvider serviceProvider,
        ILogger<PaymentTimeoutWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Payment Timeout Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var timeoutTime = DateTime.UtcNow.AddMinutes(-3);

                        var expiredOrders = await unitOfWork.Orders
                            .Where(o => o.PaymentStatus == PaymentStatus.Pending
                                     && o.OrderDate <= timeoutTime)
                            .ToListAsync(stoppingToken);

                        if (expiredOrders.Any())
                        {
                            foreach (var order in expiredOrders)
                            {
                                logger.LogInformation("Phiên thanh toán cho Đơn hàng {OrderId} đã hết hạn.", order.Id);

                                order.PaymentStatus = PaymentStatus.Failed;


                                var transaction = await unitOfWork.Payments
                                    .FirstOrDefaultAsync(t => t.OrderId == order.Id, stoppingToken);

                                if (transaction != null)
                                {
                                    transaction.Status = PaymentStatus.Failed;
                                }

                                await unitOfWork.SaveChangesAsync(stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred in PaymentTimeoutWorker.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}