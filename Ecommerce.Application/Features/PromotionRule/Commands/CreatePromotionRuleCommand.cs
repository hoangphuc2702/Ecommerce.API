using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using PromotionRuleEntity = Ecommerce.Domain.Entities.PromotionRule;

namespace Ecommerce.Application.Features.PromotionRule.Commands
{
    public record CreatePromotionRuleRequest(
    string Name,
    string Type,
    int Priority,
    Guid? TargetCategoryId,
    Guid? BuyProductId,
    int MinQuantity,
    Guid? GiftProductId,
    decimal? DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate);

    public record CreatePromotionRuleCommand(
    string Name,
    string Type,
    int Priority,
    Guid? TargetCategoryId,
    Guid? BuyProductId,
    int MinQuantity,
    Guid? GiftProductId,
    decimal? DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Result<Guid>>;

    public class CreatePromotionRuleCommandHandler : IRequestHandler<CreatePromotionRuleCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePromotionRuleCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<PromotionType>(request.Type, out var promoType))
                return Result<Guid>.Failure("Invalid promotion type.");

            var rule = new PromotionRuleEntity
            {
                Name = request.Name,
                Type = promoType,
                Priority = request.Priority,
                TargetCategoryId = request.TargetCategoryId,
                BuyProductId = request.BuyProductId,
                MinQuantity = request.MinQuantity,
                GiftProductId = request.GiftProductId,
                DiscountPercentage = request.DiscountPercentage,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            _context.PromotionRules.Add(rule);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.SuccessResult(rule.Id, "Create Promotion Rule successfully.");
        }
    }
}
