using Ecommerce.Application.Features.Order.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Queries
{
    public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;
}
