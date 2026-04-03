using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Commands
{
    public record UpdateCartItemRequest(int Quantity);
    public record UpdateCartItemCommand(Guid ProductId, int Quantity) : IRequest;
}
