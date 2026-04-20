using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.PromotionRule.DTOs
{
    public record PromotionRuleDto(
        Guid Id,
        string Name,
        string Type,
        int Priority,
        bool IsActive,
        DateTime StartDate,
        DateTime EndDate,

        Guid? TargetCategoryId,
        Guid? BuyProductId,
        int MinQuantity,

        Guid? GiftProductId,
        decimal? DiscountPercentage
    );
}
