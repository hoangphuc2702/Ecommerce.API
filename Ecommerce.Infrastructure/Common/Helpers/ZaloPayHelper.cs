using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Infrastructure.Common.Helpers
{
    public class ZaloPayHelper
    {
        public static string ComputeHmacSha256(string data, string key)
        {
            using(var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public static string GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();    
    }
}
