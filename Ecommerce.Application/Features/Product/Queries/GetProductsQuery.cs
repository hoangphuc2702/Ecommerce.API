using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Features.Product.DTOs;

namespace Ecommerce.Application.Features.Product.Queries
{
    public record GetProductsQuery(
        string? Name,
        Guid? CategoryId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<ProductDto>>;

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetProductsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name))
                .ToListAsync(cancellationToken);

            return new PaginatedList<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
        }

    }
}
