using Ecommerce.Application.Features.Coupon.Commands;
using Ecommerce.Application.Features.Coupon.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure.Services;
using MediatR;

namespace Ecommerce.API.Endpoints
{
    public static class CouponEndpoints
    {
        public static void MapCouponEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/coupons").WithTags("Coupons");
            group.MapPost("/", async (CreateCouponRequest request, ISender sender) =>
            {
                var command = new CreateCouponCommand(
                    request.Code,
                    request.DiscountType,
                    request.Value,
                    request.MinOrderValue,
                    request.StartDate,
                    request.EndDate,
                    request.UsageLimit
                );

                var result = await sender.Send(command);

                return Results.Ok(new { Success = true, CouponId = result });
            })
            .WithName("CreateCoupon")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/apply", async (ApplyCouponRequest request, ISender sender, ICurrentUserService currentUserService) =>
            {

                var userId = currentUserService.UserId;
                if (userId == null)
                    return Results.Unauthorized();

                var command = new ApplyCouponCommand(request.CouponCode, userId.Value);
                var result = await sender.Send(command);

                return Results.Ok(new { Success = true, Data = result });
            })
            .RequireAuthorization()
            .WithName("ApplyCoupon")
            .Produces<CouponDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
