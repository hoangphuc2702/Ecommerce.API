using Ecommerce.Application.Features.Category.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.Queries
{
    public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

        public GetCategoryByIdQueryHandler(IApplicationDbContext context, ILogger<GetCategoryByIdQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching category details for ID: {CategoryId}", request.Id);
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == request.Id)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} was not found.", request.Id);
                throw new NotFoundException("Category", request.Id);
            }
            _logger.LogInformation("Successfully retrieved details for category: {CategoryName}", category.Name);
            return category;
        }
    }
  }
