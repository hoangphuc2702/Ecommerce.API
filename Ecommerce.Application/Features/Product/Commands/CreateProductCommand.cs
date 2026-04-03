using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Product.Commands
{
    public record CreateProductRequest(string Name, decimal Price, string? Description, Guid CategoryId);
    public record CreateProductCommand(string Name, decimal Price, string? Description, Guid CategoryId) : IRequest<Guid>;

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<CreateProductCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to create product: {ProductName} (Price: {Price})", request.Name, request.Price);

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
            {
                _logger.LogWarning("Create Product failed: Category {CategoryId} not found.", request.CategoryId);
                throw new NotFoundException("Category", request.CategoryId);
            }

            var product = new Ecommerce.Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                CategoryId = request.CategoryId
            };

            _context.Products.Add(product);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product {ProductId} created successfully for Category {CategoryId}", product.Id, request.CategoryId);

            return product.Id;
        }
    }
}
