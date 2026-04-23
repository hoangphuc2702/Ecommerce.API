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
        ILogger<PayOSWebhookCommandHandler> logger,
        IShippingService shippingService) : IRequestHandler<PayOSWebhookCommand, object>
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
                try
                {
                    logger.LogInformation("PayOS payment confirmed. Initiating Ahamove booking for OrderId: {OrderId}", order.Id);

                    var shipResult = await shippingService.CreateShipmentAsync(order);

                    if (shipResult.IsSuccess)
                    {
                        var newShipment = new Domain.Entities.Shipment
                        {
                            OrderId = order.Id,
                            TrackingNumber = shipResult.Data.TrackingNumber,
                            PartnerCode = "AHAMOVE",
                            ServiceId = "SGN-BIKE",
                            Fee = shipResult.Data.Fee,
                            CodAmount = 0,

                            Status = ShipmentStatus.Idle,
                        };

                        order.ShippingFee = shipResult.Data.Fee;

                        await unitOfWork.Shipments.AddAsync(newShipment, cancellationToken);
                        logger.LogInformation("Ahamove shipment booked successfully via Webhook! Tracking Number: {Tracking}", newShipment.TrackingNumber);
                    }
                    else
                    {
                        logger.LogError("Ahamove booking failed for OrderId {OrderId}: {Message}", order.Id, shipResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Payment succeeded but Ahamove booking failed for OrderId {OrderId}", order.Id);
                }

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