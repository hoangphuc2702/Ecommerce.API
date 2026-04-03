using Ecommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public record UpdateOrderStatusRequest(OrderStatus NewStatus, string? Note);
    public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus, string? Note) : IRequest<bool>;
}
