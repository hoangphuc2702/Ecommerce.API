using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Shipping.DTOs
{
    public record ShippingEstimateDto(
        string ServiceId,
        string ServiceName,
        decimal Fee
    );
}
