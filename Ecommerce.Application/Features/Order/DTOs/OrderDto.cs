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
        decimal ShippingFee,
        string PhoneNumber,
        List<OrderItemDto> Items
    );

    public class CheckoutResponse
    {
        public Guid OrderId { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
