using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Application.Features.ZaloPay.DTOs
{
    public record ZaloPayCallbackRequest(
        [property: JsonPropertyName("data")] string Data,
        [property: JsonPropertyName("mac")] string Mac
    );

    public record ZaloPayDataResponse(
        [property: JsonPropertyName("app_id")] int AppId,
        [property: JsonPropertyName("app_trans_id")] string AppTransId,
        [property: JsonPropertyName("zp_trans_id")] long ZpTransId,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("app_time")] long AppTime
    );

    public record ZaloPayCreateResponse(
        [property: JsonPropertyName("return_code")] int ReturnCode,
        [property: JsonPropertyName("return_message")] string ReturnMessage,
        [property: JsonPropertyName("order_url")] string OrderUrl,
        [property: JsonPropertyName("zp_trans_token")] string ZpTransToken
    );
}
