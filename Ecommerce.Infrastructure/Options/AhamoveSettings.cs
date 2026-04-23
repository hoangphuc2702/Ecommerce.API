using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Options
{
    public class AhamoveSettings
    {
        
        public string ApiKey { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public double WarehouseLat { get; set; }
        public double WarehouseLng { get; set; }
        public string DefaultServiceId { get; set; } = "SGN-BIKE";
    }
}