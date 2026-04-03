using Ecommerce.Application.Features.Cart.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Queries
{
    public record GetCartQuery() : IRequest<CartDto>;
    
}
