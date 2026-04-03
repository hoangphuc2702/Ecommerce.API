using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.Commands
{
    public record UpdateCategoryRequest(string Name, string? Description);
    public record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Unit>;

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;
        public UpdateCategoryCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<UpdateCategoryCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category {CategoryId} to new name: {NewName}", request.Id, request.Name);

            var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Update failed: Category {CategoryId} not found.", request.Id);
                throw new NotFoundException("Category", request.Id);
            }
            category.Name = request.Name;
            category.Description = request.Description;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category {CategoryId} updated successfully.", request.Id);
            return Unit.Value;
        }

    }
}