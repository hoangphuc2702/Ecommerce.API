using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.Common.Models
{
    public record AhamoveOrderDetails(
        [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        [property: JsonPropertyName("total_fee")] decimal TotalFee,

        [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        [property: JsonPropertyName("total_pay")] decimal TotalPay
    );

    public record AhamoveOrderResponse(
        [property: JsonPropertyName("order_id")] string OrderId,

        [property: JsonPropertyName("order")] AhamoveOrderDetails OrderInfo
    )
    {
        [JsonIgnore]
        public decimal ActualFee =>
            (OrderInfo?.TotalPay > 0) ? OrderInfo.TotalPay : (OrderInfo?.TotalFee ?? 0);
    }
}
