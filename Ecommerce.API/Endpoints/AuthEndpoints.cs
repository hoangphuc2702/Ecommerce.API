using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Auth.Queries;
using Ecommerce.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Ecommerce.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndPoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/auth")
                           .WithTags("Authentication");

            group.MapPost("register", async ([FromBody] RegisterCommand command, [FromServices] ISender sender) =>
            {
                var userId = await sender.Send(command);

                return Results.Created($"/api/v1/users/{userId}", new { id = userId, message = "User registered successfully!" });
            })
            .WithName("RegisterUser")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPost("login", async ([FromBody] LoginQuery query, [FromServices] ISender sender) =>
            {
                var response = await sender.Send(query);
                return Results.Ok(response);
            })
            .WithName("LoginUser")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }

    }
}
