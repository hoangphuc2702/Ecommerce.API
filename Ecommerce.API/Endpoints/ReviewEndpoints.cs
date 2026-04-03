using Ecommerce.Application.Features.Review.Commands;
using Ecommerce.Application.Features.Review.Queries;
using MediatR;

namespace Ecommerce.API.Endpoints
{
    public static class ReviewEndpoints
    {
        public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1").WithTags("Reviews");

            group.MapPost("/reviews", async (PostReviewCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(new { success = true, reviewId = result });
            })
            .WithName("PostReview")
            .RequireAuthorization();

            group.MapGet("/products/{id:guid}/reviews", async (Guid id, int pageNumber, int pageSize, ISender sender) =>
            {
                return Results.Ok(await sender.Send(new GetReviewsQuery(id, pageNumber, pageSize)));
            })
            .WithName("GetProductReviews");
        }
    }
}
