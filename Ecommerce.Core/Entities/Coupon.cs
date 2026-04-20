using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Coupon : BaseAuditEntity
    {
        public string Code { get; set; } = default!;
        public DiscountType DiscountType { get; set; }
        public decimal Value { get; set; } //Discount value
        public decimal MinOrderValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool CanBeUsed()
        {
            var now = DateTime.UtcNow;
            return IsActive
                && now >= StartDate
                && now <= EndDate
                && UsedCount < UsageLimit;
        }
    }
}
