using Ecommerce.Application.Features.Category.Commands;
using Ecommerce.Application.Features.Category.DTOs;
using Ecommerce.Application.Features.Category.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ecommerce.API.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/categories")
                       .WithTags("Categories");

        group.MapGet("/", async (ISender sender) =>
        {
            var categories = await sender.Send(new GetCategoriesQuery());
            return Results.Ok(categories);
        })
        .WithName("GetCategories")
        .Produces<IEnumerable<CategoryDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateCategoryRequest request, ISender sender) =>
        {
            var command = new CreateCategoryCommand(request.Name, request.Description);
            var result = await sender.Send(command);
            return Results.Ok(new { Success = true, Id = result });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateCategory")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var category = await sender.Send(new GetCategoryByIdQuery(id));
            return Results.Ok(category);
        })
        .WithName("GetCategoryById")
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender sender) =>
        {
            var command = new UpdateCategoryCommand(id, request.Name, request.Description);
            await sender.Send(command);

            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("UpdateCategory")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteCategoryCommand(id));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("DeleteCategory")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}