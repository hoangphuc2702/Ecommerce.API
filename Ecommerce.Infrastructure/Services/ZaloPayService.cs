using Ecommerce.Application.Features.ZaloPay.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Services
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ZaloPayService> _logger;

        public ZaloPayService(IConfiguration config, HttpClient httpClient, IDistributedCache cache, ILogger<ZaloPayService> logger)
        {
            _config = config;
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> CreatePaymentUrl(Order order)
        {
            var zaloConfig = _config.GetSection("ZaloPay");
            var appTransId = $"{DateTime.Now:yyMMdd}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            var items = new[] {new {itemid = order.Id.ToString(), itemname = $"Order #{order.Id}", itemprice = Convert.ToInt64(Math.Round(order.TotalAmount, 0))} };
            var embedData = "{\"preferred_payment_method\":[]}";

            var param = new Dictionary<string, string>
            {
                { "app_id", zaloConfig["AppId"] ?? "" },
                { "app_user", "Ecommerce" },
                { "app_time", ZaloPayHelper.GetTimestamp() },
                { "amount", Convert.ToInt64(Math.Round(order.TotalAmount, 0)).ToString() },
                { "app_trans_id", appTransId },
                { "embed_data", "{\"preferred_payment_method\":[]}" },
                { "item", JsonSerializer.Serialize(items) },
                { "description", $"Order Payment #{order.Id}" },
                { "bank_code", "" },

                { "callback_url", "https://vacillant-tristful-sallie.ngrok-free.dev/api/v1/orders/callback/zalopay" }
            };

            var data = $"{param["app_id"]}|{param["app_trans_id"]}|{param["app_user"]}|{param["amount"]}|{param["app_time"]}|{param["embed_data"]}|{param["item"]}";
            param.Add("mac", ZaloPayHelper.ComputeHmacSha256(data, zaloConfig["Key1"] ?? ""));

            var content = new FormUrlEncodedContent(param);
            var response = await _httpClient.PostAsync(zaloConfig["Endpoint"], content);
            var result = await response.Content.ReadFromJsonAsync<ZaloPayCreateResponse>();

            if (result?.ReturnCode == 1)
            {
                await _cache.SetStringAsync($"order_payment_{appTransId}", "Pending", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                });
                return result.OrderUrl;
            }
            _logger.LogError("ZaloPay Create Order Failed: {Code} - {Message}. OrderId: {OrderId}",
                result?.ReturnCode, result?.ReturnMessage, order.Id);
            throw new BadRequestException($"Unable to create ZaloPay payment: {result?.ReturnMessage}");
        }

        public bool ValidateCallback(string data, string mac)
        {
            try
            {
                var key2 = _config["ZaloPay:Key2"];
                var expectedMac = ZaloPayHelper.ComputeHmacSha256(data, key2!);

                return string.Equals(expectedMac, mac, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ZaloPay Callback verification");
                return false;
            }
        }
    }
}
