using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Product.Commands;

public record UpdateProductRequest(string Name, decimal Price, string? Description, Guid CategoryId);

public record UpdateProductCommand(Guid Id, string Name, decimal Price, string? Description, Guid CategoryId) : IRequest<Unit>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
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

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
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
        product.CategoryId = request.CategoryId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated successfully.", request.Id);

        return Unit.Value;
    }
}