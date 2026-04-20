using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.PromotionRule.Commands
{
    public record UpdatePromotionRuleRequest(Guid Id,
    string Name,
    string Type,
    int Priority,
    bool IsActive,
    Guid? TargetCategoryId,
    Guid? BuyProductId,
    int MinQuantity,
    Guid? GiftProductId,
    decimal? DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate);

    public record UpdatePromotionRuleCommand(
    Guid Id,
    string Name,
    string Type,
    int Priority,
    bool IsActive,
    Guid? TargetCategoryId,
    Guid? BuyProductId,
    int MinQuantity,
    Guid? GiftProductId,
    decimal? DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Result<Unit>>;

    public class UpdatePromotionRuleCommandHandler : IRequestHandler<UpdatePromotionRuleCommand, Result<Unit>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePromotionRuleCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(UpdatePromotionRuleCommand request, CancellationToken cancellationToken)
        {
            var rule = await _context.PromotionRules
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (rule == null) return Result<Unit>.Failure("Promotion rule not found.");

            if (!Enum.TryParse<PromotionType>(request.Type, out var promoType))
                return Result<Unit>.Failure("Invalid promotion type.");

            rule.Name = request.Name;
            rule.Type = promoType;
            rule.Priority = request.Priority;
            rule.IsActive = request.IsActive;
            rule.TargetCategoryId = request.TargetCategoryId;
            rule.BuyProductId = request.BuyProductId;
            rule.MinQuantity = request.MinQuantity;
            rule.GiftProductId = request.GiftProductId;
            rule.DiscountPercentage = request.DiscountPercentage;
            rule.StartDate = request.StartDate;
            rule.EndDate = request.EndDate;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.SuccessResult(Unit.Value, "Update Promotion Rule successfully.");
        }
    }
}
