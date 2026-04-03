using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.DTOs
{
    public record CouponDto(string CouponCode, decimal SubTotal, decimal DiscountAmount, decimal FinalPrice);
    
}
