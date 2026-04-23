using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.PromotionRule.Commands;
using Ecommerce.Application.Features.PromotionRule.DTOs;
using Ecommerce.Application.Features.PromotionRule.Queries;
using MediatR;

namespace Ecommerce.API.Endpoints
{
    public static class PromotionRuleEndpoint
    {
        public static void MapPromotionRuleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/promotion-rules")
                           .WithTags("PromotionRules");

            group.MapGet("/", async ([AsParameters] GetPromotionRuleQuery query, ISender sender) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPromotionRules")
            .Produces<Result<PaginatedList<PromotionRuleDto>>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var promotionRule = await sender.Send(new GetPromotionRuleByIdQuery(id));
                return Results.Ok(promotionRule);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPromotionRuleById")
            .Produces<PromotionRuleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", async (CreatePromotionRuleRequest request, ISender sender) =>
            {
                var command = new CreatePromotionRuleCommand(
                    request.Name,
                    request.Type,
                    request.Priority,
                    request.TargetCategoryId,
                    request.BuyProductId,
                    request.MinQuantity,
                    request.GiftProductId,
                    request.DiscountPercentage,
                    request.StartDate,
                    request.EndDate);

                var result = await sender.Send(command);
                return Results.Ok(new { Success = true, Id = result });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("CreatePromotionRule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", async (Guid id, UpdatePromotionRuleRequest request, ISender sender) =>
            {
                var command = new UpdatePromotionRuleCommand(
                    id,
                    request.Name,
                    request.Type,
                    request.Priority,
                    request.IsActive,
                    request.TargetCategoryId,
                    request.BuyProductId,
                    request.MinQuantity,
                    request.GiftProductId,
                    request.DiscountPercentage,
                    request.StartDate,
                    request.EndDate);

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("UpdatePromotionRule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
