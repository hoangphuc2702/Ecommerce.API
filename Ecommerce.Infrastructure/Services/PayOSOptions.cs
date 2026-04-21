using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Services
{
    public class PayOSOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
    }
}
