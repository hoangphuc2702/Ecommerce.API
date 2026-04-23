using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Shipment : BaseAuditEntity
    {
        public Guid OrderId { get; set; }
        public string? TrackingNumber { get; set; }
        public string PartnerCode { get; set; } = "AHAMOVE";
        public string? ServiceId { get; set; }
        public decimal Fee { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Idle;
        public decimal CodAmount { get; set; }
        public string? AhamoveOrderId { get; set; }

        public Order Order { get; set; } = null!;
    }
}
