using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Features.PromotionRule.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.PromotionRule.Queries
{
    public record GetPromotionRuleQuery(
        string? Name,
        string? SortBy = null,
        bool IsAscending = false,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<PromotionRuleDto>>;

    public class GetPromotionRuleQueryHandler : IRequestHandler<GetPromotionRuleQuery, PaginatedList<PromotionRuleDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPromotionRuleQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<PromotionRuleDto>> Handle(GetPromotionRuleQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PromotionRules
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsAscending
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAtUtc)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PromotionRuleDto(
                p.Id,
                p.Name,
                p.Type.ToString(),
                p.Priority,
                p.IsActive,
                p.StartDate,
                p.EndDate,
                p.TargetCategoryId,
                p.BuyProductId,
                p.MinQuantity,
                p.GiftProductId,
                p.DiscountPercentage
            ))
            .ToListAsync(cancellationToken);

            return new PaginatedList<PromotionRuleDto>(items, totalCount, request.PageNumber, request.PageSize);
        }

    }
}
