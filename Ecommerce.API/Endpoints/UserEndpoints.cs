using Ecommerce.Application.Features.Users.Queries;
using MediatR;

namespace Ecommerce.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/users")
                           .WithTags("Users")
                           .RequireAuthorization();

            group.MapGet("/profile", async (ISender sender) =>
            {
                var result = await sender.Send(new GetProfileQuery());
                return Results.Ok(result);
            })
            .WithName("GetProfile");
        }
    }
}
