using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Features.ProductVariant.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using ProductVariantEntity = Ecommerce.Domain.Entities.ProductVariant;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.ProductVariant.Commands
{
    public record CreateProductVariantRequest(
    Guid ProductId,
    string Sku,
    string Color,
    string Size,
    decimal Price,
    int Stock);

    public record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    string Color,
    string Size,
    decimal Price,
    int Stock) : IRequest<Result<Guid>>;

    public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        public CreateProductVariantCommandHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<Guid>> Handle(CreateProductVariantCommand request, CancellationToken ct)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == request.ProductId, ct);
            if (!productExists) return Result<Guid>.Failure("Product not found.");

            var variant = new ProductVariantEntity
            {
                ProductId = request.ProductId,
                Sku = request.Sku,
                Color = request.Color,
                Size = request.Size,
                Price = request.Price,
                Stock = request.Stock
            };
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync(ct);
            return Result<Guid>.SuccessResult(variant.Id, "Product variant created successfully.");
        }
    }
}
