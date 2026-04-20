using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Service.Promotion
{
    public interface IPromotionEngine
    {
        PromotionEvaluation Evaluate(IEnumerable<CartItem> cartItems, List<PromotionRule> activeRules);
    }

    public class PromotionEngine : IPromotionEngine
    {
        public PromotionEvaluation Evaluate(IEnumerable<CartItem> cartItems, List<PromotionRule> activeRules)
        {
            var evaluation = new PromotionEvaluation();
            var subtotal = cartItems.Sum(i => i.Quantity * i.Product.Price);

            var sortedRules = activeRules.OrderByDescending(r => r.Priority).ToList();

            foreach (var rule in sortedRules)
            {
                if (rule.Type == PromotionType.CategoryDiscount)
                {
                    var targetItems = cartItems.Where(i => i.Product.CategoryId == rule.TargetCategoryId);
                    foreach (var item in targetItems)
                    {
                        var discount = (item.Quantity * item.Product.Price) * (rule.DiscountPercentage ?? 0 / 100);
                        evaluation.TotalPromotionDiscount += discount;
                        evaluation.AppliedRuleNames.Add(rule.Name);
                    }
                }
                else if (rule.Type == PromotionType.BuyXGetY)
                {
                    var item = cartItems.FirstOrDefault(i => i.ProductId == rule.BuyProductId);
                    if (item != null && item.Quantity >= rule.MinQuantity)
                    {
                        evaluation.Gifts.Add(new GiftItem(rule.GiftProductId!.Value, 1));
                        evaluation.AppliedRuleNames.Add(rule.Name);
                    }
                }
            }

            evaluation.FinalSubtotal = subtotal - evaluation.TotalPromotionDiscount;
            return evaluation;
        }
    }
}
