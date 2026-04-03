using Ecommerce.Application.Features.Cart.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Queries
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCartQueryHandler> _logger;
        public GetCartQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetCartQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning("Get Cart failed: User identification is missing from the token.");
                throw new UnauthorizedException("User identification is missing. Please log in.");
            }

            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            // Xử lý trường hợp giỏ hàng rỗng
            if (cart == null)
            {
                _logger.LogInformation("User {UserId} requested cart, but it does not exist. Returning empty cart.", userId);
                return new CartDto
                {
                    UserId = userId.Value,
                    Items = new List<CartItemDto>()
                };
            }

            _logger.LogInformation("Successfully retrieved cart for User {UserId} with {ItemCount} items.", userId, cart.Items.Count);

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };
        }
    }
}
