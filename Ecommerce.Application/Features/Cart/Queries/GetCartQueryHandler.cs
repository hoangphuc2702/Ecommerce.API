using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Cart.DTOs;
using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Features.Product.DTOs;
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
    public record GetCartQuery() : IRequest<Result<CartDto>>;
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<CartDto>>
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

        public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning("Get Cart failed: User identification is missing from the token.");
                return Result<CartDto>.Failure(new Error(
                    "User.Unauthorized",
                    "User identification is missing. Please log in."
                ));
            }

            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null)
            {
                _logger.LogInformation("User {UserId} requested cart, but it does not exist. Returning empty cart.", userId);
                return Result<CartDto>.SuccessResult(new CartDto { UserId = userId.Value, });
            }

            _logger.LogInformation("Successfully retrieved cart for User {UserId} with {ItemCount} items.", userId, cart.Items.Count);

            var now = DateTime.UtcNow;
            var activeRules = await _context.PromotionRules
                .AsNoTracking()
                .Where(r => r.IsActive && r.StartDate <= now && r.EndDate >= now)
                .OrderByDescending(r => r.Priority)
                .ToListAsync(cancellationToken);

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = new List<CartItemDto>()
            };

            foreach (var item in cart.Items)
            {
                var originalPrice = item.Product.Price;
                var finalPrice = originalPrice;
                string? promoName = null;
                decimal discountAmount = 0;


                var appliedRule = activeRules.FirstOrDefault(r =>
                    (r.BuyProductId == item.ProductId && item.Quantity >= r.MinQuantity) ||
                    (r.TargetCategoryId == item.Product.CategoryId && item.Quantity >= r.MinQuantity));

                if (appliedRule != null)
                {

                    if (appliedRule.DiscountPercentage.HasValue)
                    {
                        discountAmount = originalPrice * (appliedRule.DiscountPercentage.Value / 100);
                        finalPrice = originalPrice - discountAmount;

                    }


                    if (appliedRule.GiftProductId.HasValue && appliedRule.GiftProductId != null)
                    {
                        int giftQuantity = (item.Quantity / appliedRule.MinQuantity);

                        var product = await _context.Products
                            .AsNoTracking()
                            .Include(p => p.Category)
                            .Where(p => p.Id == appliedRule.GiftProductId)
                            .Select(p => new ProductDto(
                                p.Id,
                                p.Name,
                                p.Price,
                                p.Description,
                                p.Stock,
                                p.Category.Name
                            ))
                            .FirstOrDefaultAsync(cancellationToken);

                        if (giftQuantity > 0)
                        {
                            cartDto.Items.Add(new CartItemDto
                            {
                                ProductId = appliedRule.GiftProductId.Value,
                                ProductName = $"[QUÀ TẶNG] {product.Name}",
                                Quantity = giftQuantity,
                                OriginalPrice = product.Price,
                                Price = 0,
                                DiscountAmount = product.Price,
                                AppliedPromotionName = appliedRule.Name,
                                ProductImageUrl = null
                            });
                        }
                    }

                    promoName = appliedRule.Name;

                }

                cartDto.Items.Add(new CartItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    OriginalPrice = originalPrice,
                    Price = finalPrice,
                    DiscountAmount = discountAmount,
                    AppliedPromotionName = promoName
                });
            }

            return Result<CartDto>.SuccessResult(cartDto);
        }
    }
}
