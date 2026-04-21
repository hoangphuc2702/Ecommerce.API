using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Product.Commands;
using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Features.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ecommerce.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products")
                       .WithTags("Products");

        group.MapGet("/", async ([AsParameters] GetProductsQuery query, ISender sender) =>
        {
            var products = await sender.Send(query);
            var response = Result<PaginatedList<ProductDto>>.SuccessResult(products);
            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .Produces<Result<PaginatedList<ProductDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var product = await sender.Send(new GetProductByIdQuery(id));
            return Results.Ok(product);
        })
        .WithName("GetProductById")
        .Produces<ProductDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProductRequest request, ISender sender) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Price,
                request.Description,
                request.Stock,
                request.CategoryId);

            var result = await sender.Send(command);
            return Results.Ok(new { Success = true, Id = result });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateProduct")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Price,
                request.Description,
                request.Stock,
                request.CategoryId);

            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", async (Guid id, bool isDeleted, ISender sender) =>
        {
            var command = new UpdateProductStatusCommand(id, isDeleted);

            var result = await sender.Send(command);
            return Results.Ok(new
            {
                Success = true,
                Id = result
            });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateProductStatus");
    }
}