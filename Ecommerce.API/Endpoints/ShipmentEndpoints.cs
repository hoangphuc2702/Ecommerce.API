using Ecommerce.Application.Features.Shipping.Commands;
using Ecommerce.Application.Features.Shipping.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Endpoints
{
    public static class ShipmentEndpoints
    {
        public static void MapShipmentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/shipments")
                           .WithTags("Shipments");

            group.MapPost("/{orderId:guid}", async (Guid orderId, ISender sender) =>
            {
                var command = new CreateShipmentCommand(orderId);
                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithName("CreateAhamoveShipment")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            
            group.MapPost("/webhook/ahamove", async ([FromBody] AhamoveWebhookPayload payload, ISender sender) =>
            {
                Console.WriteLine(payload);
                var command = new ProcessAhamoveWebhookCommand(payload);
                var result = await sender.Send(command);

                return Results.Ok();
            })
            .WithName("HandleAhamoveWebhook")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
