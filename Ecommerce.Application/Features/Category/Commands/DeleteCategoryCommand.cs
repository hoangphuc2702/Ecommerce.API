using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.Commands
{
    public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;
        public DeleteCategoryCommandHandler(
            IApplicationDbContext context, 
            IUnitOfWork unitOfWork,
            ILogger<DeleteCategoryCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete category with ID: {CategoryId}", request.Id);
            var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Delete failed: Category {CategoryId} not found.", request.Id);
                throw new NotFoundException("Category", request.Id);
            }
            _context.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category {CategoryId} deleted successfully.", request.Id);
            return Unit.Value;
        }
    }

}
