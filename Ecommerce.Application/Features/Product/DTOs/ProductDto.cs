using Ecommerce.Application.Features.ProductVariant.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Product.DTOs
{
    public record ProductDto(Guid Id,
    string Name,
    decimal Price,
    string? Description,
    int Stock,
    string CategoryName,
    double Weight,
    double Length,
    double Width,
    double Height
    );

    public record ProductDetailDto(Guid Id,
    string Name,
    decimal Price,
    string? Description,
    int Stock,
    Guid CategoryId,
    string CategoryName,
    double AverageRating,
    int TotalReviews,
    List<ProductVariantDto> Variants,
    double Weight,
    double Length,
    double Width,
    double Height

    );
}
