using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.Common.Models
{
    public record AhamoveTokenResponse(
    [property: JsonPropertyName("token")] string Token);
}
