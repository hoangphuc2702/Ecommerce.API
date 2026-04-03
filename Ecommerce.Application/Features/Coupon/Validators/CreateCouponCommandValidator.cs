using Ecommerce.Application.Features.Coupon.Commands;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;


namespace Ecommerce.Application.Features.Coupon.Validators
{
    public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
    {
        public CreateCouponCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Coupon code is required.")
                .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.");

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

            RuleFor(x => x.MinOrderValue)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order value cannot be negative.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be strictly after the start date.");

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0).WithMessage("Usage limit must be greater than 0.");
        }
    }
}
