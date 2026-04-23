using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Commands
{
    public record UpdateCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int UsageLimit,
    bool IsActive);

    public record UpdateCouponCommand(
        Guid Id,
        string Code,
        DiscountType DiscountType,
        decimal Value,
        decimal MinOrderValue,
        DateTime StartDate,
        DateTime EndDate,
        int UsageLimit,
        bool IsActive) : IRequest<Result<Guid>>;

    public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCouponCommandHandler> _logger;

        public UpdateCouponCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<UpdateCouponCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update coupon ID: {CouponId}", request.Id);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (coupon == null)
            {
                _logger.LogWarning("Update failed: Coupon {CouponId} not found.", request.Id);
                return Result<Guid>.Failure($"Coupon with this ID {request.Id} not found.");
            }

            if (coupon.Code.ToLower() != request.Code.ToLower())
            {
                var codeExists = await _context.Coupons
                    .AsQueryable()
                    .AnyAsync(c => c.Code.ToLower() == request.Code.ToLower() && c.Id != request.Id, cancellationToken);

                if (codeExists)
                {
                    _logger.LogWarning("Update failed: Coupon code {Code} already exists.", request.Code);
                    return Result<Guid>.Failure($"Coupon '{request.Code}' has already been used.");
                }
            }

            coupon.Code = request.Code.ToUpper();
            coupon.DiscountType = request.DiscountType;
            coupon.Value = request.Value;
            coupon.MinOrderValue = request.MinOrderValue;
            coupon.StartDate = request.StartDate;
            coupon.EndDate = request.EndDate;
            coupon.UsageLimit = request.UsageLimit;
            coupon.IsActive = request.IsActive;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Coupon {CouponId} updated successfully.", coupon.Id);

            return Result<Guid>.SuccessResult(coupon.Id, "Coupon updated successfully!");
        }
    }
}
