using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Application.Features.Shipping.DTOs
{
    public record AhamoveWebhookPayload(
    [property: JsonPropertyName("_id")] string TrackingNumber,
    [property: JsonPropertyName("status")] string Status,

    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("total_fee")] decimal? TotalFee,

    [property: JsonPropertyName("path")] List<AhamovePathPayload>? Paths
);

    public record AhamovePathPayload(
        [property: JsonPropertyName("address")] string Address,
        [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [property: JsonPropertyName("cod")] decimal CodAmount
    );
}
