using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public record CancelOrderCommand(Guid OrderId) : IRequest<bool>;
    
}
