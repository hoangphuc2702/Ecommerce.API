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
    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCouponCommandHandler> _logger;

        public CreateCouponCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<CreateCouponCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to create coupon with code: {Code}", request.Code);

            var codeExists = await _context.Coupons
                .AnyAsync(c => c.Code.ToLower() == request.Code.ToLower(), cancellationToken);

            if (codeExists)
            {
                _logger.LogWarning("Create coupon failed: Code {Code} already exists.", request.Code);
                throw new BadRequestException($"Coupon code '{request.Code}' already exists.");
            }

            var coupon = new Ecommerce.Domain.Entities.Coupon
            {
                Code = request.Code.ToUpper(),
                DiscountType = request.DiscountType,
                Value = request.Value,
                MinOrderValue = request.MinOrderValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                IsActive = true
            };

            _context.Coupons.Add(coupon);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Coupon {CouponId} created successfully.", coupon.Id);

            return coupon.Id;
        }
    }
}
