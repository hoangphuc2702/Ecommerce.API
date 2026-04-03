using Ecommerce.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.DTOs
{
    public record ReviewDto(Guid Id, string UserName, int Rating, string Comment, DateTime CreatedAt);

    public record ReviewSummaryDto(double AverageRating, PaginatedList<ReviewDto> PaginatedReviews);
}
