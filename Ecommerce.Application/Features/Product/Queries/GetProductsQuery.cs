using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq; // Thêm Linq để xài Select
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Features.Product.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Product.Queries
{
    public record GetProductsQuery(
        string? Name,
        Guid? CategoryId = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        string? SortBy = null,
        bool IsAscending = false,
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
                .AsQueryable();

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice);

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));

            query = request.SortBy?.ToLower() switch
            {
                "price" => request.IsAscending
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),
                "name" => request.IsAscending
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAtUtc)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Description ?? string.Empty,
                    p.Stock,
                    p.Category.Name,
                    p.Weight,
                    p.Length,
                    p.Width,
                    p.Height
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedList<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}