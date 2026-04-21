using Ecommerce.Application.Common.Models;
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
    public class ZaloPayService : IPaymentService
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

        public async Task<Result<string>> CreatePaymentLink(PaymentRequest request)
        {
            try
            {
                var zaloConfig = _config.GetSection("ZaloPay");

                var appTransId = $"{DateTime.Now:yyMMdd}_{request.OrderCode}";

                var items = request.Items?.Select(x => (object)new {
                    itemid = x.Name,
                    itemname = x.Name,
                    itemprice = Convert.ToInt64(Math.Round(x.Price, 0)),
                    itemquantity = x.Quantity
                }).ToList() ?? new List<object>();

                var param = new Dictionary<string, string>
            {
                { "app_id", zaloConfig["AppId"] ?? "" },
                { "app_user", request.BuyerName },
                { "app_time", ZaloPayHelper.GetTimestamp() },
                { "amount", Convert.ToInt64(Math.Round(request.Amount, 0)).ToString() },
                { "app_trans_id", appTransId },
                { "embed_data", "{\"preferred_payment_method\":[]}" },
                { "item", JsonSerializer.Serialize(items) },
                { "description", request.Description },
                { "bank_code", "" },
                { "callback_url", zaloConfig["CallbackUrl"] ?? "" }
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

                    return Result<string>.SuccessResult(result.OrderUrl, "Create ZaloPay link successfully.");
                }

                return Result<string>.Failure(new Error("ZaloPay.Error", result?.ReturnMessage ?? "Unknown error"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZaloPay Exception for Order: {OrderCode}", request.OrderCode);
                return Result<string>.Failure(new Error("ZaloPay.Exception", ex.Message));
            }
        }

        public Task<Result<PaymentStatusInfo>> GetPaymentStatus(long orderCode)
        {
            return Task.FromResult(Result<PaymentStatusInfo>.Failure(
                new Error("ZaloPay.NotImplemented", "Not implemented")));
        }

        public Task<Result<bool>> CancelPaymentLink(long orderCode)
        {
            return Task.FromResult(Result<bool>.SuccessResult(true));
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
