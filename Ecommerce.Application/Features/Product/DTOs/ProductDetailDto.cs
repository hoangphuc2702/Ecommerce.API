using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Product.DTOs
{
    public record ProductDetailDto(Guid Id,
    string Name,
    decimal Price,
    string? Description,
    Guid CategoryId,
    string CategoryName,
        double AverageRating,
    int TotalReviews
    );
}
