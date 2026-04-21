using Ecommerce.Application.Features.PayOS.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PayOS.Models.Webhooks;

namespace Ecommerce.API.Endpoints
{
    public static class PaymentEndpoints
    {
        public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/payment/payos")
                           .WithTags("Payment (PayOS)");

            group.MapPost("/create", async (CreatePayOSLinkCommand command, ISender sender) =>
            {
                var url = await sender.Send(command);
                return Results.Ok(new { checkoutUrl = url });
            })
            .WithName("CreatePayOSLink");

            group.MapPost("/webhook", async (Webhook webhookBody, ISender sender) =>
            {
                if (webhookBody.Data == null)
                {
                    return Results.Ok(new { message = "Webhook link is active" });
                }

                var result = await sender.Send(new PayOSWebhookCommand(webhookBody));
                return Results.Ok(result);
            });

            group.MapGet("/checkout/success", (long orderCode) =>
            {
                return Results.Content(
                    $@"<html>
                        <head><title>Payment Successful</title></head>
                        <body style='font-family: Arial, sans-serif; text-align: center; padding-top: 50px;'>
                            <h1 style='color: #28a745;'> Payment Successful!</h1>
                            <p>Thank you for your purchase. Your Order ID: <b>{orderCode}</b></p>
                            <p>We are processing your order and will notify you soon.</p>
                            <hr style='width: 50%; margin: 20px auto;'>
                            <a href='/' style='text-decoration: none; color: #007bff; font-weight: bold;'>Return to Home</a>
                        </body>
                    </html>", "text/html", System.Text.Encoding.UTF8);
            })
            .AllowAnonymous();

            group.MapGet("/checkout/cancel", (long orderCode) =>
            {
                return Results.Content(
                    $@"<html>
                        <head><title>Payment Cancelled</title></head>
                        <body style='font-family: Arial, sans-serif; text-align: center; padding-top: 50px;'>
                            <h1 style='color: #dc3545;'> Payment Cancelled</h1>
                            <p>The payment process for Order <b>{orderCode}</b> has been cancelled.</p>
                            <p>No worries! Your order is still saved. You can try to pay again in your order history.</p>
                            <hr style='width: 50%; margin: 20px auto;'>
                            <a href='/' style='text-decoration: none; color: #007bff; font-weight: bold;'>Return to Store</a>
                        </body>
                    </html>", "text/html", System.Text.Encoding.UTF8);
                        })
            .AllowAnonymous();
        }
    }
}