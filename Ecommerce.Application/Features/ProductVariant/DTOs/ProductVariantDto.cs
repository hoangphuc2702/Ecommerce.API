using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ProductVariant.DTOs
{
    public record ProductVariantDto(
        Guid Id,
        string Sku,
        string Color,
        string Size,
        decimal? Price,
        int Stock
    );
}
