using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.DTOs
{
    public record CouponDto(
        Guid Id,
        string Code,
        DiscountType DiscountType,
        decimal Value,
        decimal MinOrderValue,
        DateTime StartDate,
        DateTime EndDate,
        int UsageLimit,
        int UsedCount,
        bool IsActive,
        bool IsExpired 
    );

    public record ApplyCouponDto(
        string CouponCode,
        decimal OriginalPrice,
        decimal PromotionDiscount,
        decimal CouponDiscount,
        decimal TotalDiscount,
        decimal FinalPrice,
        List<string> AppliedRules, 
        List<GiftItemDto> Gifts
    );

    public record GiftItemDto(Guid ProductId, string ProductName, int Quantity);
}
