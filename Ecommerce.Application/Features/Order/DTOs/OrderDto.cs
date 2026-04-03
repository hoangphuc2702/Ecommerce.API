using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.DTOs
{
    public record OrderDto(
        Guid Id,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TotalAmount,
        string Status,
        DateTime OrderDate,
        string ShippingAddress,
        string PhoneNumber,
        List<OrderItemDto> Items
    );
}
