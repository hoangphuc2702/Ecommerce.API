using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Service.Promotion;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Commands
{

    public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, Result<ApplyCouponDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApplyCouponCommandHandler> _logger;
        private readonly IPromotionEngine _promotionEngine;

        public ApplyCouponCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<ApplyCouponCommandHandler> logger,
            IPromotionEngine promotionEngine)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _promotionEngine = promotionEngine;
        }

        public async Task<Result<ApplyCouponDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User {UserId} is applying coupon: {CouponCode}", request.UserId, request.CouponCode);

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart == null || !cart.Items.Any())
                return Result<ApplyCouponDto>.Failure("Your cart is empty.");

            decimal originalPrice = cart.Items.Sum(i => i.Quantity * i.Product.Price);

            var now = DateTime.UtcNow;
            var allActivePromotions = await _context.PromotionRules
            .Where(r => r.IsActive && r.StartDate <= now && r.EndDate >= now)
            .ToListAsync(cancellationToken);

            var promoEvaluation = _promotionEngine.Evaluate(cart.Items, allActivePromotions);
            decimal subtotalAfterPromotion = promoEvaluation.FinalSubtotal;

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == request.CouponCode.ToLower(), cancellationToken);

            if (coupon == null || !coupon.CanBeUsed())
                return Result<ApplyCouponDto>.Failure("Invalid or inactive coupon code.");

            if (subtotalAfterPromotion < coupon.MinOrderValue)
                return Result<ApplyCouponDto>.Failure($"Minimum order value of {coupon.MinOrderValue} is required to use this coupon.");

            decimal couponDiscount = 0;
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                couponDiscount = subtotalAfterPromotion * (coupon.Value / 100);
                if (coupon.MaxDiscountAmount.HasValue && couponDiscount > coupon.MaxDiscountAmount.Value)
                    couponDiscount = coupon.MaxDiscountAmount.Value;
            }
            else
            {
                couponDiscount = coupon.Value;
            }

            if (couponDiscount > subtotalAfterPromotion)
                couponDiscount = subtotalAfterPromotion;

            cart.AppliedCouponCode = coupon.Code;

            if (promoEvaluation.Gifts.Any())
            {
                _logger.LogInformation("User {UserId} received {Count} gifts from promotion rules.", request.UserId, promoEvaluation.Gifts.Count);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var giftIds = promoEvaluation.Gifts.Select(g => g.ProductId).ToList();
            var giftProducts = await _context.Products
                .Where(p => giftIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

            var giftDtos = promoEvaluation.Gifts.Select(g => new GiftItemDto(
                g.ProductId,
                giftProducts.GetValueOrDefault(g.ProductId, "Gift product"),
                g.Quantity
            )).ToList();

            var result = new ApplyCouponDto(
                CouponCode: coupon.Code,
                OriginalPrice: originalPrice,
                PromotionDiscount: promoEvaluation.TotalPromotionDiscount,
                CouponDiscount: couponDiscount,
                TotalDiscount: promoEvaluation.TotalPromotionDiscount + couponDiscount,
                FinalPrice: subtotalAfterPromotion - couponDiscount,
                AppliedRules: promoEvaluation.AppliedRuleNames,
                Gifts: giftDtos
            );

            return Result<ApplyCouponDto>.SuccessResult(result, "Coupon applied successfully.");
        }
    }
}
