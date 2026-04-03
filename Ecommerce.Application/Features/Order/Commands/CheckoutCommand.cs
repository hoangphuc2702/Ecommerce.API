using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public record CheckoutRequest(string ShippingAddress, string PhoneNumber);
    public record CheckoutCommand(string ShippingAddress, string PhoneNumber) : IRequest<Guid>;
}
