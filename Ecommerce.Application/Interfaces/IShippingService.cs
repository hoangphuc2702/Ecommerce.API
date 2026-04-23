using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces
{
    public interface IShippingService
    {
        Task<Result<decimal>> GetEstimatedFeeAsync(string destinationAddress, double latitude, double longitude, List<OrderItemDto> items, string? serviceId = null);
        Task<Result<(string TrackingNumber, decimal Fee)>> CreateShipmentAsync(Order order);
    }
}
