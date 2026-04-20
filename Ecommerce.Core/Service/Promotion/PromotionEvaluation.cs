using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Service.Promotion
{
    public class PromotionEvaluation
    {
        public decimal TotalPromotionDiscount { get; set; }
        public decimal FinalSubtotal { get; set; }
        public List<string> AppliedRuleNames { get; set; } = new();
        public List<GiftItem> Gifts { get; set; } = new();
    }

    public record GiftItem(Guid ProductId, int Quantity);
}
