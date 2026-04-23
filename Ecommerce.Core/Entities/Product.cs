using Ecommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Product : BaseAuditEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public double Weight { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public byte[]? Version { get; set; }
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
