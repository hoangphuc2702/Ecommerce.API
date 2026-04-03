using Ecommerce.Application.Features.Order.Commands;
using Ecommerce.Application.Features.Order.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ecommerce.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("api/v1/orders")
                           .WithTags("Orders (User)");

        userGroup.MapPost("/checkout", async (CheckoutRequest request, ISender sender) =>
        {
            var command = new CheckoutCommand(request.ShippingAddress, request.PhoneNumber);
            var orderId = await sender.Send(command);
            return Results.Ok(new { Success = true, OrderId = orderId });
        })
        .RequireAuthorization()
        .WithName("Checkout")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        userGroup.MapGet("/history", async (ISender sender) =>
        {
            var orders = await sender.Send(new GetOrderHistoryQuery());
            return Results.Ok(new { Success = true, Data = orders });
        })
        .RequireAuthorization()
        .WithName("GetOrderHistory")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        userGroup.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var order = await sender.Send(new GetOrderByIdQuery(id));
            return Results.Ok(new { Success = true, Data = order });
        })
        .RequireAuthorization()
        .WithName("GetOrderDetails")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        userGroup.MapPut("/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            await sender.Send(new CancelOrderCommand(id));
            return Results.Ok(new { Success = true, Message = "Order cancelled successfully." });
        })
        .RequireAuthorization()
        .WithName("CancelOrder")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);


        var adminGroup = app.MapGroup("api/v1/admin/orders")
                            .WithTags("Orders (Admin)");

        adminGroup.MapPut("/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, ISender sender) =>
        {
            var command = new UpdateOrderStatusCommand(id, request.NewStatus, request.Note);
            await sender.Send(command);
            return Results.Ok(new { Success = true, Message = "Order status updated successfully." });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateOrderStatus")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}