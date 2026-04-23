using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Shipping.Commands
{
    public record CreateShipmentCommand(Guid OrderId) : IRequest<Result<string>>;

    public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShippingService _shippingService;
        private readonly ILogger<CreateShipmentCommandHandler> _logger;

        public CreateShipmentCommandHandler(
            IUnitOfWork unitOfWork,
            IShippingService shippingService,
            ILogger<CreateShipmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _shippingService = shippingService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing shipment creation for OrderId: {OrderId}", request.OrderId);

            var order = await _unitOfWork.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product)
                            .Include(o => o.Shipment)
                            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Shipment creation failed. Order {OrderId} not found.", request.OrderId);
                return Result<string>.Failure(new Error("Order.NotFound", $"Order {request.OrderId} not found."));
            }

            if (order.Shipment != null)
            {
                _logger.LogWarning("Order {OrderId} already has a shipment tracking number: {TrackingNumber}", request.OrderId, order.Shipment.TrackingNumber);
                return Result<string>.Failure(new Error("Order.AlreadyShipped", "This order has already been shipped."));
            }

            var shipResult = await _shippingService.CreateShipmentAsync(order);

            if (!shipResult.IsSuccess)
            {
                _logger.LogError("Ahamove API failed for OrderId {OrderId}.", request.OrderId);
                return Result<string>.Failure(shipResult.Error);
            }
            _logger.LogInformation(">>> DEBUG COD - Method: '{Method}', TotalAmount: {Total}",
            order.PaymentMethod, order.TotalAmount);

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = shipResult.Data.TrackingNumber,
                PartnerCode = "AHAMOVE",
                Fee = shipResult.Data.Fee,
                CodAmount = order.PaymentMethod == "COD" ? order.TotalAmount : 0,
                Status = ShipmentStatus.Idle 
            };

            order.Shipment = shipment;
            order.Status = OrderStatus.Shipped;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created shipment {TrackingNumber} for OrderId {OrderId}", order.Shipment.TrackingNumber, order.Id);

            return Result<string>.SuccessResult(shipResult.Data.TrackingNumber, "Shipment created successfully.");
        }
    }
}