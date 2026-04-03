using Ecommerce.Application.Features.Category.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.Queries
{
    public record GetCategoriesQuery() : IRequest<List<CategoryDto>>;
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetCategoriesQueryHandler> _logger;

        public GetCategoriesQueryHandler(IApplicationDbContext context, ILogger<GetCategoriesQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all categories from database.");

            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} categories.", categories.Count);

            return categories;
        }
    }
}
