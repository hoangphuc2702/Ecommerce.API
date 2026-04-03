using Ecommerce.Application.Features.Coupon.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Coupon.Validators
{
    public class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
    {
        public ApplyCouponCommandValidator()
        {
            RuleFor(x => x.CouponCode)
                .NotEmpty().WithMessage("Coupon code is required.");
        }
    }
}
