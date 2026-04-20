using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace Ecommerce.Application.Features.ProductVariant.Commands
{
    public record DeleteProductVariantCommand(Guid Id) : IRequest<Result<Unit>>;

    public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result<Unit>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteProductVariantCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<Unit>> Handle(DeleteProductVariantCommand request, CancellationToken ct)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == request.Id, ct);

            if (variant == null)
                return Result<Unit>.Failure("Product Variant not found.");

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync(ct);

            return Result<Unit>.SuccessResult(Unit.Value, "Product Variant deleted successfully.");
        }
    }
}
