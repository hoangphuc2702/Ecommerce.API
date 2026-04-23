using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Shipping.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Shipping.Commands
{
    public record ProcessAhamoveWebhookCommand(AhamoveWebhookPayload Payload) : IRequest<Result<bool>>;

    public class ProcessAhamoveWebhookCommandHandler : IRequestHandler<ProcessAhamoveWebhookCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessAhamoveWebhookCommandHandler> _logger;

        public ProcessAhamoveWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ProcessAhamoveWebhookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(ProcessAhamoveWebhookCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Webhook received from Ahamove for Tracking Number: {TrackingNumber} with Status: {Status}",
                request.Payload.TrackingNumber, request.Payload.Status);

            var order = await _unitOfWork.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Shipment != null && o.Shipment.TrackingNumber == request.Payload.TrackingNumber, cancellationToken);

            if (order == null || order.Shipment == null)
            {
                _logger.LogWarning("Webhook processing failed: Order with tracking number {TrackingNumber} not found.", request.Payload.TrackingNumber);
                return Result<bool>.Failure(new Error("Webhook.OrderNotFound", $"Order with tracking number {request.Payload.TrackingNumber} not found."));
            }

            order.Status = request.Payload.Status.ToOrderStatus(order.Status);

            order.Shipment.Status = request.Payload.Status.ToUpper() switch
            {
                "IDLE" => ShipmentStatus.Idle,

                "ASSIGNING" => ShipmentStatus.PickingUp,
                "ACCEPTED" => ShipmentStatus.PickingUp,

                "IN PROCESS" => ShipmentStatus.Shipping,
                "IN_PROCESS" => ShipmentStatus.Shipping,

                "COMPLETED" => ShipmentStatus.Delivered,
                "CANCELLED" => ShipmentStatus.Cancelled,
                "FAILED" => ShipmentStatus.Failed,
                _ => order.Shipment.Status
            };

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully synchronized OrderId {OrderId} status to {NewStatus} based on Ahamove webhook.", order.Id, order.Status);

            return Result<bool>.SuccessResult(true, "Order status synchronized with Ahamove successfully.");
        }
    }
}