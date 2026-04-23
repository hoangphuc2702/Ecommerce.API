using Ecommerce.Application.Features.Order.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public record CheckoutRequest(
        string CustomerName,
        string ShippingAddress,
        string PhoneNumber,
        string PaymentMethod,
        string ServiceId,
        double Latitude,
        double Longitude
    );

    public record CheckoutCommand(
        string CustomerName,
        string ShippingAddress,
        string PhoneNumber,
        string PaymentMethod,
        string ServiceId,
        double Latitude,
        double Longitude
    ) : IRequest<CheckoutResponse>;
}
