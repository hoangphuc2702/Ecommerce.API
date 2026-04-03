using Ecommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Category : BaseAuditEntity
    {
        public string Name { get; set; } = string.Empty;    
        public string? Description { get; set; } = string.Empty;
         public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
