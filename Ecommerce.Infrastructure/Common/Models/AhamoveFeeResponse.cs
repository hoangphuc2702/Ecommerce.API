using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.Common.Models
{
    public record AhamoveFeeResponse(
     [property: JsonPropertyName("total_price")] decimal TotalPrice);
}
