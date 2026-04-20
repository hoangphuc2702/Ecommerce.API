using Ecommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class ProductVariant : BaseAuditEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Color { get; set; } = null!;

        public decimal? Price { get; set; }
        public int Stock { get; set; }
    }
}
