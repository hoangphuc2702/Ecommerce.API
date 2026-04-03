using Ecommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Commands
{
    public record CreateCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int UsageLimit);

    public record CreateCouponCommand(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int UsageLimit) : IRequest<Guid>;
}
