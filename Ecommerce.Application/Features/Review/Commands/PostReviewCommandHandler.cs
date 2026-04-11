using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.Commands
{
    public class PostReviewCommandHandler : IRequestHandler<PostReviewCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PostReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(PostReviewCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue) throw new UnauthorizedException("Authentication required.");

            var canReview = await _context.Orders
                .AsNoTracking()
                .AnyAsync(o => o.UserId == userId.Value
                            && o.Status == OrderStatus.Completed
                            && o.OrderItems.Any(oi => oi.ProductId == request.ProductId),
                          cancellationToken);

            if (!canReview)
                throw new BadRequestException("You can only review products from completed orders.");

            var review = new Ecommerce.Domain.Entities.Review
            {
                ProductId = request.ProductId,
                UserId = userId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(review, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return review.Id;
        }
    }
}
