using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Review.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.Queries
{
    public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, ReviewSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetReviewsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ReviewSummaryDto> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == request.ProductId);

            var totalReviews = await query.CountAsync(cancellationToken);

            double averageRating = totalReviews > 0
                ? await query.AverageAsync(r => r.Rating, cancellationToken)
                : 0;

            var reviews = await query
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new ReviewDto(
                    r.Id,
                    r.User.Name,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt))
                .ToListAsync(cancellationToken);
            var paginatedReviews = new PaginatedList<ReviewDto>(
                reviews,
                totalReviews,
                request.PageNumber,
                request.PageSize
            );

            return new ReviewSummaryDto(Math.Round(averageRating, 1), paginatedReviews);
        }
    }
}
