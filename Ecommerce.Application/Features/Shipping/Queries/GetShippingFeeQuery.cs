using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Service.Promotion;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Shipping.Queries
{
    public record GetShippingFeeQuery(
        string DestinationAddress,
        string? CouponCode,
        double Latitude,
        double Longitude,
        string? ServiceId
    ) : IRequest<ShippingFeePreviewResponse>;

    public class ShippingFeePreviewResponse
    {
        public string DestinationAddress { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public List<string> Gifts { get; set; } = new();
        public string? AppliedCouponCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal PromotionDiscount { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class GetShippingFeeQueryHandler : IRequestHandler<GetShippingFeeQuery, ShippingFeePreviewResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IShippingService _shippingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPromotionEngine _promotionEngine;

        public GetShippingFeeQueryHandler(
            IApplicationDbContext context,
            IShippingService shippingService,
            ICurrentUserService currentUserService,
            IPromotionEngine promotionEngine)
        {
            _context = context;
            _shippingService = shippingService;
            _currentUserService = currentUserService;
            _promotionEngine = promotionEngine;
        }

        public async Task<ShippingFeePreviewResponse> Handle(GetShippingFeeQuery request, CancellationToken cancellationToken)
        {
            var response = new ShippingFeePreviewResponse
            {
                DestinationAddress = request.DestinationAddress
            };

            var userId = _currentUserService.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                return response;
            }

            var cart = await _context.Carts
                            .Include(c => c.Items)
                                .ThenInclude(ci => ci.Product)
                            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null || !cart.Items.Any())
            {
                return response;
            }

            foreach (var item in cart.Items)
            {
                var price = item.Product?.Price ?? 0;
                var itemSubTotal = price * item.Quantity;

                response.Items.Add(new OrderItemDto(
                    item.ProductId,
                    item.Product?.Name ?? string.Empty,
                    item.Quantity,
                    price,
                    itemSubTotal,
                    item.Product?.Weight ?? 0,
                    item.Product?.Length ?? 0,
                    item.Product?.Width ?? 0,
                    item.Product?.Height ?? 0
                ));

                response.SubTotal += itemSubTotal;
            }

            var now = DateTime.UtcNow;
            var activePromotions = await _context.PromotionRules
                .Where(r => r.IsActive && r.StartDate <= now && r.EndDate >= now)
                .ToListAsync(cancellationToken);

            var promoEvaluation = _promotionEngine.Evaluate(cart.Items, activePromotions);
            response.PromotionDiscount = promoEvaluation.TotalPromotionDiscount;

            var giftIds = promoEvaluation.Gifts.Select(g => g.ProductId).ToList();
            var giftProducts = await _context.Products
                .Where(p => giftIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            response.Gifts = promoEvaluation.Gifts
                .Select(g => giftProducts.GetValueOrDefault(g.ProductId, "Gift product"))
                .ToList();

            decimal subtotalAfterPromo = promoEvaluation.FinalSubtotal;
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToLower() == request.CouponCode.ToLower(), cancellationToken);

                if (coupon != null && coupon.CanBeUsed() && subtotalAfterPromo >= coupon.MinOrderValue)
                {
                    response.AppliedCouponCode = coupon.Code.ToUpper();
                    decimal cDiscount = 0;
                    if (coupon.DiscountType == DiscountType.Percentage)
                    {
                        cDiscount = subtotalAfterPromo * (coupon.Value / 100);
                        if (coupon.MaxDiscountAmount.HasValue && cDiscount > coupon.MaxDiscountAmount.Value)
                            cDiscount = coupon.MaxDiscountAmount.Value;
                    }
                    else
                    {
                        cDiscount = coupon.Value;
                    }
                    response.CouponDiscount = Math.Min(cDiscount, subtotalAfterPromo);
                }
            }

            var shippingResult = await _shippingService.GetEstimatedFeeAsync(request.DestinationAddress, request.Latitude, request.Longitude, response.Items, request.ServiceId);
            response.ShippingFee = shippingResult.IsSuccess ? shippingResult.Data : 0;

            response.TotalAmount = subtotalAfterPromo - response.CouponDiscount + response.ShippingFee;
            if (response.TotalAmount < 0) response.TotalAmount = 0;

            return response;
        }
    }
}