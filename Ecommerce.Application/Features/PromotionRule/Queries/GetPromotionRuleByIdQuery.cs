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
    public record GetPromotionRuleByIdQuery(Guid Id) : IRequest<Result<PromotionRuleDto>>;

    public class GetPromotionRuleByIdQueryHandler : IRequestHandler<GetPromotionRuleByIdQuery, Result<PromotionRuleDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPromotionRuleByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PromotionRuleDto>> Handle(GetPromotionRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var rule = await _context.PromotionRules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (rule == null) return Result<PromotionRuleDto>.Failure("Promotion rule not found.");

            var dto = new PromotionRuleDto(
                rule.Id,
                rule.Name,
                rule.Type.ToString(),
                rule.Priority,
                rule.IsActive,
                rule.StartDate,
                rule.EndDate,
                rule.TargetCategoryId,
                rule.BuyProductId,
                rule.MinQuantity,
                rule.GiftProductId,
                rule.DiscountPercentage
            );

            return Result<PromotionRuleDto>.SuccessResult(dto);
        }
    }
}
