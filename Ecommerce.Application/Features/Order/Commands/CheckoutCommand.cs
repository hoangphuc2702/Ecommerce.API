using Ecommerce.Application.Features.Order.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public record CheckoutRequest(
        string ShippingAddress,
        string PhoneNumber,
        string PaymentMethod,
        double Latitude,
        double Longitude
    );

    public record CheckoutCommand(
        string ShippingAddress,
        string PhoneNumber,
        string PaymentMethod,
        double Latitude,
        double Longitude
    ) : IRequest<CheckoutResponse>;
}
