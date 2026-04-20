using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class PromotionRule : BaseAuditEntity
    {
        public string Name { get; set; } = default!;
        public PromotionType Type { get; set; }
        public int Priority { get; set; }
        public Guid? TargetCategoryId { get; set; }
        public Guid? BuyProductId { get; set; }
        public int MinQuantity { get; set; }
        public Guid? GiftProductId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
