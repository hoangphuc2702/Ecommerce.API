using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Product.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDetailDto(
                p.Id,
                p.Name,
                p.Price,
                p.Description ?? "",
                p.CategoryId,
                p.Category.Name,
                _context.Reviews.Where(r => r.ProductId == p.Id).Any()
                    ? Math.Round(_context.Reviews.Where(r => r.ProductId == p.Id).Average(r => (double)r.Rating), 1)
                    : 0,
                _context.Reviews.Count(r => r.ProductId == p.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);

        return product;
    }
}