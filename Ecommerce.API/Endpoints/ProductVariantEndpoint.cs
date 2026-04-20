using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Product.Commands;
using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Features.Product.Queries;
using Ecommerce.Application.Features.ProductVariant.Commands;
using Ecommerce.Application.Features.ProductVariant.DTOs;
using Ecommerce.Application.Features.ProductVariant.Queries;
using MediatR;

namespace Ecommerce.API.Endpoints;
public static class ProductVariantEndpoints
{
    public static void MapProductVariantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/product-variants")
                       .WithTags("ProductVariants");

        group.MapGet("/", async ([AsParameters] GetVariantsByProductIdQuery query, ISender sender) =>
        {
            var productVariant = await sender.Send(query);
            return Results.Ok(productVariant);
        })
        .WithName("GetProductVariants")
        .Produces<Result<PaginatedList<ProductVariantDto>>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateProductVariantRequest request, ISender sender) =>
        {
            var command = new CreateProductVariantCommand(
                request.ProductId,
                request.Sku,
                request.Color,
                request.Size,
                request.Price,
                request.Stock);

            var result = await sender.Send(command);
            return Results.Ok(new { Success = true, Id = result });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateProductVariant")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductVariantRequest request, ISender sender) =>
        {
            var command = new UpdateProductVariantCommand(
                id,
                request.Sku,
                request.Color,
                request.Size,
                request.Price,
                request.Stock);

            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateProductVariant")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteProductVariantCommand(id));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("DeleteProductVariant")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}