using Ecommerce.Application.Common.Hubs;
using Ecommerce.Application.Features.ZaloPay.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Application.Features.ZaloPay.Commands
{
    public record UpdateZaloPayCallbackCommand(string Data, string Mac) : IRequest<object>;
    public class UpdateZaloPayCallbackHandler(
        IUnitOfWork unitOfWork,
        IZaloPayService zaloPayService,
        IHubContext<PaymentHub> hubContext,
        IDistributedCache cache,
        ILogger<UpdateZaloPayCallbackHandler> logger) : IRequestHandler<UpdateZaloPayCallbackCommand, object>
    {
        public async Task<object> Handle(UpdateZaloPayCallbackCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("ZaloPay Webhook received. Data: {Data}", request.Data);

            if (!zaloPayService.ValidateCallback(request.Data, request.Mac))
            {
                logger.LogWarning("MAC validation failed! Potential fraud attempt.");
                return new { return_code = -1, return_message = "Invalid MAC" };
            }

            var dataJson = JsonSerializer.Deserialize<ZaloPayDataResponse>(request.Data);
            var appTransId = dataJson!.AppTransId;

            var cacheKey = $"payment_processed:{appTransId}";
            if (await cache.GetStringAsync(cacheKey, cancellationToken) is not null)
            {
                return new { return_code = 1, return_message = "Success (Already Processed)" };
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var parts = appTransId.Split('_');
                if (parts.Length < 2 || !Guid.TryParse(parts[1], out var orderId))
                {
                    logger.LogError("Invalid AppTransId format: {AppTransId}", appTransId);
                    return new { return_code = -1, return_message = "Invalid AppTransId" };
                }

                var order = await unitOfWork.Orders.FindAsync(new object[] { orderId }, cancellationToken);

                if (order is null)
                {
                    logger.LogError("Order {OrderId} not found in database.", orderId);
                    return new { return_code = -1, return_message = "Order not found" };
                }

                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    await unitOfWork.CommitTransactionAsync(cancellationToken);
                    return new { return_code = 1, return_message = "Success" };
                }

                order.PaymentStatus = PaymentStatus.Paid;
                order.ZaloPayTransId = dataJson.ZpTransId.ToString();

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                await cache.SetStringAsync(cacheKey, "Success",
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) }, cancellationToken);

                await hubContext.Clients.Group(orderId.ToString())
                    .SendAsync("ReceivePaymentStatus", new
                    {
                        Status = "Success",
                        OrderId = orderId,
                        Message = "Successful"
                    }, cancellationToken);

                return new { return_code = 1, return_message = "Success" };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                logger.LogError(ex, "Error processing ZaloPay callback for {AppTransId}", appTransId);
                return new { return_code = 0, return_message = "Internal error" };
            }
        }
    }
}
