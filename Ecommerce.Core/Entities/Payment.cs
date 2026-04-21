using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class Payment : BaseAuditEntity
    {
        public Guid OrderId { get; set; }

        public string PaymentProvider { get; set; } = string.Empty;

        public string ReferenceId { get; set; } = string.Empty;

        public string? ProviderTransactionId { get; set; }

        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Message { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
