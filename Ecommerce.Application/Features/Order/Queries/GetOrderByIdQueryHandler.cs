using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Order.Queries
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(IApplicationDbContext context, ILogger<GetOrderByIdQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving order details for OrderId: {OrderId}", request.OrderId);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Shipment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} was not found in the database.", request.OrderId);
                throw new NotFoundException($"Order with ID {request.OrderId} not found.");
            }

            return new OrderDto(
                order.Id,
                order.SubTotal,
                order.DiscountAmount,
                order.TotalAmount,
                order.Status.ToString(),
                order.OrderDate,
                order.ShippingAddress ?? "N/A",
                order.ShippingFee,
                order.PhoneNumber ?? "N/A",

                order.Shipment != null ? new ShipmentDto(
                    order.Shipment.TrackingNumber ?? string.Empty,
                    order.Shipment.PartnerCode,
                    order.Shipment.Fee,
                    order.Shipment.Status.ToString()
                ) : null,

                order.OrderItems.Select(oi => new OrderItemDto(
                    oi.ProductId,
                    oi.Product?.Name ?? "Hidden Product",
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice,
                    oi.Product?.Weight ?? 0,
                    oi.Product?.Length ?? 0,
                    oi.Product?.Width ?? 0,
                    oi.Product?.Height ?? 0
                )).ToList()
            );
        }
    }
}