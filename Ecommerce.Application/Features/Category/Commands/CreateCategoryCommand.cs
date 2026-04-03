using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.Commands
{
    public record CreateCategoryRequest(string Name, string? Description);
    public record CreateCategoryCommand(string Name, string? Description) : IRequest<Guid>;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new category with name: {CategoryName}", request.Name);

            var category = new Ecommerce.Domain.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category {CategoryId} created successfully.", category.Id);
            return category.Id;
        }
    }
}
