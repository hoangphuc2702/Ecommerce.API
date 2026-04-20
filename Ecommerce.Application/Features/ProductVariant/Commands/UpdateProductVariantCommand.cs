using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ProductVariant.Commands
{
    public record UpdateProductVariantRequest(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    decimal Price,
    int Stock);

    public record UpdateProductVariantCommand(
    Guid Id,
    string Sku,
    string Color,
    string Size,
    decimal Price,
    int Stock) : IRequest<Result<Unit>>;

    public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result<Unit>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductVariantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(UpdateProductVariantCommand request, CancellationToken ct)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == request.Id, ct);

            if (variant == null)
                return Result<Unit>.Failure($"Product Variant with ID {request.Id} not found.");

            if (variant.Sku != request.Sku)
            {
                var skuExists = await _context.ProductVariants
                    .AsQueryable()
                    .AnyAsync(v => v.Sku == request.Sku && v.Id != request.Id, ct);

                if (skuExists)
                    return Result<Unit>.Failure($"SKU '{request.Sku}' is already in use by another variant.");
            }

            variant.Sku = request.Sku;
            variant.Color = request.Color;
            variant.Size = request.Size;
            variant.Price = request.Price;
            variant.Stock = request.Stock;

            await _context.SaveChangesAsync(ct);

            return Result<Unit>.SuccessResult(Unit.Value, "Product Variant updated successfully.");
        }
    }
}
