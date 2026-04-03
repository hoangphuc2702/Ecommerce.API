using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Ecommerce.Application.Features.Product.Queries
{
    public record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto>;
}
