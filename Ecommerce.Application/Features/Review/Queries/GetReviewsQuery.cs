using Ecommerce.Application.Features.Review.DTOs;
using Ecommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.Queries
{
    public record GetReviewsQuery : IRequest<ReviewSummaryDto>
    {
        public Guid ProductId { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }

        public GetReviewsQuery(Guid productId, int pageNumber, int pageSize)
        {
            ProductId = productId;
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;
            PageSize = pageSize <= 0 ? 10 : pageSize;
        }
    }
    
}
