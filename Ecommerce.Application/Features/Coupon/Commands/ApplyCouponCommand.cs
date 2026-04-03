using Ecommerce.Application.Features.Coupon.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Commands
{
    public record ApplyCouponRequest(string CouponCode);
    public record ApplyCouponCommand(string CouponCode, Guid UserId) : IRequest<CouponDto>;
}
