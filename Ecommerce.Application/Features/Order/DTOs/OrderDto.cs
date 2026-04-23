using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.DTOs
{
    public record ShipmentDto(
        string TrackingNumber,
        string PartnerCode,
        decimal Fee,
        string Status
    );

    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal SubTotal,
        double Weight,
        double Length,
        double Width,
        double Height
    );
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
        ShipmentDto? Shipment,
        List<OrderItemDto> Items
    );

    public class CheckoutResponse
    {
        public Guid OrderId { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
