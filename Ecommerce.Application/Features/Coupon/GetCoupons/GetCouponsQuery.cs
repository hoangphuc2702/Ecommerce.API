using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Cart.DTOs;
using Ecommerce.Application.Features.Cart.Queries;
using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.GetCoupons
{
    public record GetCouponsQuery : IRequest<Result<List<CouponDto>>>;

    public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, Result<List<CouponDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCouponsQueryHandler> _logger;
        public GetCouponsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetCouponsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<List<CouponDto>>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning("Get Coupons failed: User identification is missing from the token.");
                throw new UnauthorizedException("User identification is missing. Please log in.");
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

            if (user == null) throw new NotFoundException("User", userId.Value);

            var query = _context.Coupons.AsNoTracking();

            var now = DateTime.UtcNow;

            if (user.Role != "Admin")
            {
                query = query.Where(c => c.IsActive
                              && c.StartDate <= now
                              && c.EndDate >= now
                              && c.UsedCount < c.UsageLimit);
            }

            var coupons = await query
                .Select(c => new CouponDto(
                    c.Id,
                    c.Code,
                    c.DiscountType,
                    c.Value,
                    c.MinOrderValue,
                    c.StartDate,
                    c.EndDate,
                    c.UsageLimit,
                    c.UsedCount,
                    c.IsActive,
                    c.EndDate < now || c.UsedCount >= c.UsageLimit //IsExpired
                ))
                .ToListAsync(cancellationToken);

            return Result<List<CouponDto>>.SuccessResult(coupons, $"Found {coupons.Count} coupons.");
        }
    }
}
