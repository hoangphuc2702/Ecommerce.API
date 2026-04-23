using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Enums
{
    public enum ShipmentStatus
    {
        Idle = 0,
        Accepted = 1,
        PickingUp = 2,
        Shipping = 3,
        Delivered = 4,
        Failed = 5,
        Cancelled = 6
    }
}
