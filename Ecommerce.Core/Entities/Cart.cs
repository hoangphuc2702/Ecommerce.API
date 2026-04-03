using Ecommerce.Core.Entities;
using Ecommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Cart : BaseAuditEntity
    {
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public string? AppliedCouponCode { get; set; }

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
