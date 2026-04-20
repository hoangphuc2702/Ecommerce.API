using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.ProductVariant.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ProductVariant.Queries
{
    public record GetVariantsByProductIdQuery(Guid ProductId) : IRequest<Result<List<ProductVariantDto>>>;

    public class GetVariantsByProductIdQueryHandler : IRequestHandler<GetVariantsByProductIdQuery, Result<List<ProductVariantDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetVariantsByProductIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductVariantDto>>> Handle(GetVariantsByProductIdQuery request, CancellationToken ct)
        {
            var variants = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == request.ProductId)
                .Select(v => new ProductVariantDto(
                    v.Id,
                    v.Sku,
                    v.Color,
                    v.Size,
                    v.Price,
                    v.Stock
                ))
                .ToListAsync(ct);

            return Result<List<ProductVariantDto>>.SuccessResult(variants);
        }
    }
}
