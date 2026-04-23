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

                        var timeoutTime = DateTime.UtcNow.AddMinutes(-10);

                        var expiredOrders = await unitOfWork.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product) 
                            .Where(o => o.PaymentStatus == PaymentStatus.Pending
                                     && o.OrderDate <= timeoutTime)
                            .ToListAsync(stoppingToken);

                        if (expiredOrders.Any())
                        {
                            foreach (var order in expiredOrders)
                            {
                                logger.LogInformation("Cancel order {OrderId} and return it to inventory due to payment being overdue by 10 minutes", order.Id);

                                order.PaymentStatus = PaymentStatus.Failed;
                                order.Status = OrderStatus.Cancelled; 

                                foreach (var item in order.OrderItems)
                                {
                                    if (item.Product != null)
                                    {
                                        item.Product.Stock += item.Quantity;
                                    }
                                }

                                var transaction = await unitOfWork.Payments
                                    .FirstOrDefaultAsync(t => t.OrderId == order.Id, stoppingToken);

                                if (transaction != null)
                                {
                                    transaction.Status = PaymentStatus.Failed;
                                }
                            }

                            await unitOfWork.SaveChangesAsync(stoppingToken);
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