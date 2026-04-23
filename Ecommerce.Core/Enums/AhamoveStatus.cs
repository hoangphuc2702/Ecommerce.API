using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Enums
{
    public static class AhamoveStatus
    {
        public static OrderStatus ToOrderStatus(this string ahamoveStatus, OrderStatus currentStatus)
        {
            if (string.IsNullOrWhiteSpace(ahamoveStatus))
                return currentStatus;

            return ahamoveStatus.ToUpperInvariant() switch
            {
                "ASSIGNING" => OrderStatus.Processing,
                "ACCEPTED" => OrderStatus.Processing,
                "IN PROCESS" => OrderStatus.Shipped,
                "COMPLETED" => OrderStatus.Completed,
                "CANCELLED" => OrderStatus.Cancelled,

                _ => currentStatus
            };
        }
    }
}
