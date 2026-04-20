using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Queries
{
    public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, List<OrderDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetOrderHistoryQueryHandler> _logger; 

        public GetOrderHistoryQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetOrderHistoryQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<OrderDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized access attempt to order history.");
                throw new UnauthorizedException("Please log in to view your order history.");
            }

            _logger.LogInformation("Fetching order history for User {UserId}", userId);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);

            var orderDtos = orders.Select(o => new OrderDto(
                o.Id,
                o.SubTotal,
                o.DiscountAmount,
                o.TotalAmount,
                o.Status.ToString(),
                o.OrderDate,
                o.ShippingAddress,
                o.ShippingFee,
                o.PhoneNumber,
                o.OrderItems.Select(oi => new OrderItemDto(
                    oi.ProductId,
                    oi.Product?.Name ?? "Unknown Product",
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice
                )).ToList()
            )).ToList();

            _logger.LogInformation("Successfully fetched {Count} orders for User {UserId}", orderDtos.Count, userId);

            return orderDtos;
        }
    }
}
