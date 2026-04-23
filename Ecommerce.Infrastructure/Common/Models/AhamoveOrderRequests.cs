using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.Common.Models
{
    public record PackageItemDTO(
         [property: JsonPropertyName("weight")] double Weight,
         [property: JsonPropertyName("length")] double Length,
         [property: JsonPropertyName("width")] double Width,
         [property: JsonPropertyName("height")] double Height
    );

    public record DropoffDTO(
        [property: JsonPropertyName("lat")] double Lat,
        [property: JsonPropertyName("lng")] double Lng,
        [property: JsonPropertyName("address")] string Address,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("mobile")] string Mobile
    );

    public record DeliveryPayloadDTO(
        [property: JsonPropertyName("serviceId")] string ServiceId,
        [property: JsonPropertyName("dropoff")] DropoffDTO Dropoff,
        [property: JsonPropertyName("items")] List<PackageItemDTO> Items,
        [property: JsonPropertyName("routeOptimized")] bool RouteOptimized
    );
}
