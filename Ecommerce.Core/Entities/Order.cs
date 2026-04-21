using Ecommerce.Core.Entities;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Order : BaseAuditEntity
    {
        public Guid UserId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PromotionDiscount { get; set; }
        public decimal CouponDiscount { get; set; }
        public string? AppliedPromotionNotes { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public Order(Guid id) : this()
        {
            Id = id;
        }

        public Order()
        {
        }

    }
}
