using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Commands
{
    public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, CouponDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApplyCouponCommandHandler> _logger;

        public ApplyCouponCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<ApplyCouponCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CouponDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User {UserId} is applying coupon: {CouponCode}", request.UserId, request.CouponCode);

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty.");

            decimal subTotal = cart.Items.Sum(c => c.Quantity * c.Product.Price);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == request.CouponCode.ToLower(), cancellationToken);

            if (coupon == null || !coupon.IsActive)
                throw new BadRequestException("Invalid or inactive coupon code.");

            if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
                throw new BadRequestException("Coupon is expired or not yet active.");

            if (coupon.UsedCount >= coupon.UsageLimit)
                throw new BadRequestException("Coupon usage limit has been reached.");

            if (subTotal < coupon.MinOrderValue)
                throw new BadRequestException($"Minimum order value of {coupon.MinOrderValue} is required to use this coupon.");

            decimal discountAmount = 0;
            if (coupon.DiscountType == Domain.Enums.DiscountType.Percentage)
            {
                discountAmount = subTotal * (coupon.Value / 100);
            }
            else if (coupon.DiscountType == Domain.Enums.DiscountType.FixedAmount)
            {
                discountAmount = coupon.Value;
            }

            if (discountAmount > subTotal)
                discountAmount = subTotal;

            decimal finalPrice = subTotal - discountAmount;

            cart.AppliedCouponCode = coupon.Code;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Coupon {Code} applied to Cart {CartId}. Discount: {Discount}, Final: {Final}",
                coupon.Code, cart.Id, discountAmount, finalPrice);

            return new CouponDto(coupon.Code, subTotal, discountAmount, finalPrice);
        }
    }
}
