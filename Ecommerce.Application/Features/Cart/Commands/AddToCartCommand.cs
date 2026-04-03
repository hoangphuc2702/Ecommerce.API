using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Commands
{
    public record AddToCartRequest(Guid ProductId, int Quantity);
    public record AddToCartCommand(Guid ProductId, int Quantity) : IRequest<Guid>;
}
