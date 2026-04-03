using Ecommerce.Application.Features.Cart.Commands;
using Ecommerce.Application.Features.Cart.DTOs;
using Ecommerce.Application.Features.Cart.Queries;
using Ecommerce.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ecommerce.API.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/carts")
                       .WithTags("Carts");

        group.MapGet("/", async (ISender sender) =>
        {
            var cart = await sender.Send(new GetCartQuery());
            return Results.Ok(cart);
        })
        .RequireAuthorization()
        .WithName("GetCart")
        .Produces<CartDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);


        group.MapPost("/add", async (AddToCartRequest request, ISender sender, System.Security.Claims.ClaimsPrincipal user) =>
        {
            if (user.IsInRole("Admin"))
            {
                return Results.Forbid(); 
            }
            var command = new AddToCartCommand(request.ProductId, request.Quantity);
            var result = await sender.Send(command);

            return Results.Ok(new { Success = true, Id = result });
        })
        .RequireAuthorization()
        .WithName("AddToCart")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);


        group.MapPut("/items/{productId:guid}", async (Guid productId, UpdateCartItemRequest request, ISender sender, System.Security.Claims.ClaimsPrincipal user) =>
        {
            if (user.IsInRole("Admin")) return Results.Forbid();
            var command = new UpdateCartItemCommand(productId, request.Quantity);

            await sender.Send(command);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("UpdateCartItem")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/items/{productId:guid}", async (Guid productId, ISender sender, System.Security.Claims.ClaimsPrincipal user) =>
        {
            if (user.IsInRole("Admin")) return Results.Forbid();
            await sender.Send(new DeleteCartCommand(productId));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("RemoveFromCart")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}