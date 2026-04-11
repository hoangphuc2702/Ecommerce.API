using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Product.DTOs
{
    public record ProductDto(Guid Id,
    string Name,
    decimal Price,
    int Stock,
    string CategoryName
    );
   
}
