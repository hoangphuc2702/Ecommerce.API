using Ecommerce.Application.Features.Shipping.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Shipping.Validators
{
    public class GetShippingFeeQueryValidator : AbstractValidator<GetShippingFeeQuery>
    {
        public GetShippingFeeQueryValidator()
        {
            RuleFor(x => x.DestinationAddress)
                .NotEmpty().WithMessage("Destination address is required.")
                .MaximumLength(500).WithMessage("Destination address must not exceed 500 characters.");

            // RuleFor(x => x.CouponCode)
            //     .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.");
        }
    }
}
