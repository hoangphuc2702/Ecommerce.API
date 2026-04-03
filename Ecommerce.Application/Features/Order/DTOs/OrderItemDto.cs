using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.DTOs
{
    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal SubTotal
    );
}
