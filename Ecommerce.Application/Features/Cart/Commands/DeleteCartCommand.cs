using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Commands
{
    public record DeleteCartCommand(Guid ProductId) : IRequest;
}
