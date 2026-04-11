using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Product.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Product.Commands;

public record UpdateProductRequest(string Name, decimal Price, string? Description, int Stock, Guid CategoryId);

public record UpdateProductCommand(Guid Id, string Name, decimal Price, string? Description, int Stock, Guid CategoryId) : IRequest<Result<ProductDetailDto>>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductDetailDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product {ProductId} with new data.", request.Id);

        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Update failed: Product {ProductId} not found.", request.Id);
            throw new NotFoundException("Product", request.Id);
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            _logger.LogWarning("Update failed: Target Category {CategoryId} does not exist.", request.CategoryId);
            throw new NotFoundException("Category", request.CategoryId);
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.Description = request.Description;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId, cancellationToken);

        var reviewsQuery = _context.Reviews.Where(r => r.ProductId == product.Id);

        var totalReviews = await reviewsQuery.CountAsync(cancellationToken);
        var averageRating = totalReviews > 0
            ? await reviewsQuery.AverageAsync(r => (double)r.Rating, cancellationToken)
            : 0.0;

        var updatedProductDto = new ProductDetailDto(
            product.Id,
            product.Name,
            product.Price,
            product.Description ?? "",
            product.Stock,
            product.CategoryId,
            category?.Name ?? "N/A",
            AverageRating: Math.Round(averageRating, 1),
            TotalReviews: totalReviews
        );

        _logger.LogInformation("Product {ProductId} updated successfully.", request.Id);

        return Result<ProductDetailDto>.SuccessResult(updatedProductDto, "Update product successfully!");
    }
}