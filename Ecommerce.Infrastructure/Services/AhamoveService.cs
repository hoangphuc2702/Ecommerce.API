using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Common.Models;
using Ecommerce.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Services;

public class AhamoveService : IShippingService
{
    private readonly HttpClient _httpClient;
    private readonly AhamoveSettings _settings;
    private readonly ILogger<AhamoveService> _logger;
    private string? _cachedToken;

    public AhamoveService(
        HttpClient httpClient,
        IOptions<AhamoveSettings> settings,
        ILogger<AhamoveService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        var baseUrl = string.IsNullOrEmpty(_settings.BaseUrl)
            ? "https://partner-apistg.ahamove.com/"
            : _settings.BaseUrl.EndsWith("/") ? _settings.BaseUrl : _settings.BaseUrl + "/";

        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private async Task<Result<string>> GetTokenAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken))
                return Result<string>.SuccessResult(_cachedToken);

            var response = await _httpClient.PostAsJsonAsync("v3/accounts/token", new
            {
                api_key = _settings.ApiKey,
                mobile = _settings.Mobile
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                return Result<string>.Failure(new Error("Ahamove.AuthError", errorDetail));
            }

            var result = await response.Content.ReadFromJsonAsync<AhamoveTokenResponse>();
            _cachedToken = result?.Token;
            return Result<string>.SuccessResult(_cachedToken ?? "", "OK");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error("Ahamove.AuthException", ex.Message));
        }
    }

    public async Task<Result<decimal>> GetEstimatedFeeAsync(string destinationAddress, double latitude, double longitude, List<OrderItemDto> items, string? serviceId = null)
    {
        try
        {
            var tokenResult = await GetTokenAsync();
            if (!tokenResult.IsSuccess) return Result<decimal>.Failure(tokenResult.Error);

            double totalWeight = items.Sum(i => (i.Weight > 0 ? i.Weight : 0.5) * i.Quantity);

            var pathData = new[] {
                new { address = _settings.ShopAddress, lat = (double)_settings.WarehouseLat, lng = (double)_settings.WarehouseLng },
                new { address = destinationAddress, lat = latitude, lng = longitude }
            };
            string pathJson = JsonSerializer.Serialize(pathData);

            var selectedServiceId = serviceId ?? _settings.DefaultServiceId;

            var url = $"https://apistg.ahamove.com/v1/order/estimated_fee?" +
                      $"token={Uri.EscapeDataString(tokenResult.Data)}&" +
                      $"service_id={Uri.EscapeDataString(selectedServiceId)}&" +
                      $"path={Uri.EscapeDataString(pathJson)}&" +
                      $"weight={totalWeight}&" +
                      $"order_time=0&" +
                      $"payment_method=BALANCE";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ahamove V1 Fee Error: {Content}", errorBody);
                return Result<decimal>.Failure(new Error("Ahamove.FeeError", errorBody));
            }

            var result = await response.Content.ReadFromJsonAsync<AhamoveFeeResponse>();
            return Result<decimal>.SuccessResult(result?.TotalPrice ?? 0, "Success");
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(new Error("Ahamove.FeeException", ex.Message));
        }
    }

    public async Task<Result<(string TrackingNumber, decimal Fee)>> CreateShipmentAsync(Order order)
    {
        try
        {
            var tokenResult = await GetTokenAsync();
            if (!tokenResult.IsSuccess) return Result<(string, decimal)>.Failure(tokenResult.Error);

            _logger.LogInformation("Creating Ahamove V3 shipment for Order: {OrderId}", order.Id);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Data);

            var packageItems = order.OrderItems.Select(item => new {
                name = item.Product?.Name ?? "Product",
                quantity = item.Quantity,
                weight = (double)(item.Product?.Weight ?? 0.5)
            }).ToList();

            var path = new[] {
                new {
                    address = _settings.ShopAddress,
                    lat = (double)_settings.WarehouseLat,
                    lng = (double)_settings.WarehouseLng,
                    short_name = "Inventory",
                    name = "Ecommerce",
                    mobile = _settings.Mobile,
                    cod = 0.0
                },
                new {
                    address = order.ShippingAddress,
                    lat = (double)order.Latitude,
                    lng = (double)order.Longitude,
                    short_name = "Customer",
                    name = order.CustomerName ?? "Customer",
                    mobile = order.PhoneNumber,
                    cod = (double)order.TotalAmount
                }
            };

            var payload = new
            {
                service_id = !string.IsNullOrEmpty(order.ServiceId)
                             ? order.ServiceId
                             : _settings.DefaultServiceId,
                path = path,
                items = packageItems,
                order_time = 0,
                payment_method = "BALANCE",
                remarks = $"Order #{order.Id.ToString()[..8]}"
            };

            var response = await _httpClient.PostAsJsonAsync("v3/orders", payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ahamove V3 Create Order Failed: {Content}", errorContent);
                return Result<(string, decimal)>.Failure(new Error("Ahamove.OrderError", errorContent));
            }

            var result = await response.Content.ReadFromJsonAsync<AhamoveOrderResponse>();
            return Result<(string TrackingNumber, decimal Fee)>.SuccessResult(
                (result?.OrderId ?? "", result?.ActualFee ?? 0),
                "Success"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Ahamove shipment.");
            return Result<(string, decimal)>.Failure(new Error("Ahamove.CreateException", ex.Message));
        }
    }
}