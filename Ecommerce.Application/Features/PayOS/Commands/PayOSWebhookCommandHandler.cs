using Ecommerce.Application.Common.Hubs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.Webhooks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.PayOS.Commands
{
    public record PayOSWebhookCommand(Webhook WebhookBody) : IRequest<object>;

    public class PayOSWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        PayOSClient payOSClient,
        IHubContext<PaymentHub> hubContext,
        ILogger<PayOSWebhookCommandHandler> logger) : IRequestHandler<PayOSWebhookCommand, object>
    {
        public async Task<object> Handle(PayOSWebhookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var verifiedData = await payOSClient.Webhooks.VerifyAsync(request.WebhookBody);

                long orderCode = verifiedData.OrderCode;

                await unitOfWork.BeginTransactionAsync(cancellationToken);

                var transaction = await unitOfWork.Payments
                    .Include(t => t.Order)
                        .ThenInclude(o => o.OrderItems)
                    .FirstOrDefaultAsync(t => t.ReferenceId == orderCode.ToString(), cancellationToken);

                if (transaction is null || transaction.Order is null)
                {
                    logger.LogError("PayOS Webhook - Transaction or Order for OrderCode {OrderCode} not found.", orderCode);
                    return new { success = false, message = "Transaction or Order not found" };
                }

                var order = transaction.Order;

                if (transaction.Status == PaymentStatus.Paid)
                {
                    await unitOfWork.CommitTransactionAsync(cancellationToken);
                    return new { success = true, message = "Already paid" };
                }

                transaction.Status = PaymentStatus.Paid;

                transaction.ProviderTransactionId = verifiedData.Reference ?? string.Empty;

                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Processing;
                /*
                foreach (var item in order.OrderItems)
                {
                    var product = await unitOfWork.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                    }
                }

                var shipment = new Domain.Entities.Shipment
                {
                    OrderId = order.Id,
                    TrackingNumber = "GHN-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ShippingProvider = "GiaoHangNhanh",
                    Status = ShipmentStatus.ReadyToPick,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3)
                };
                await unitOfWork.Shipments.AddAsync(shipment, cancellationToken);
                */

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                await hubContext.Clients.Group(order.Id.ToString())
                    .SendAsync("ReceivePaymentStatus", new
                    {
                        Status = "Success",
                        OrderId = order.Id,
                        Message = "PayOS Payment Successful"
                    }, cancellationToken);

                return new { success = true, message = "Webhook processed successfully" };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                logger.LogError(ex, "Error processing PayOS Webhook or Invalid Signature");
                return new { success = false, message = "Invalid webhook or Internal error" };
            }
        }
    }
}