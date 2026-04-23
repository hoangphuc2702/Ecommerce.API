using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Auth.Login;
using Ecommerce.Application.Features.Auth.Logout;
using Ecommerce.Application.Features.Auth.RefreshToken;
using Ecommerce.Application.Features.Auth.Register;
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

            group.MapPost("register", async ([FromBody] RegisterCommand command, ISender sender) =>
            {
                var userId = await sender.Send(command);

                return Results.Created($"/api/v1/users/{userId}", new { id = userId, message = "User registered successfully!" });
            })
            .WithName("RegisterUser")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPost("login", async ([FromBody] LoginCommand command, ISender sender) =>
            {
                var response = await sender.Send(command);
                return Results.Ok(response);
            })
            .WithName("LoginUser")
            .Produces<Result<LoginResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("refreshtoken", async (ISender sender) =>
            {
                var response = await sender.Send(new RefreshTokenCommand());
                return Results.Ok(response);
            })
            .WithName("RefreshToken")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("logout", async (ISender sender) =>
            {
                var response = await sender.Send(new LogoutCommand());
                return Results.Ok(response);
            })
            .WithName("LogoutUser")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }

    }
}
