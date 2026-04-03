using Ecommerce.Application.Features.Order.Commands;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Ecommerce.Application.Features.Order.Validators
{
    public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
    {
        public CheckoutCommandValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required.")
                .MaximumLength(500);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\d{10,11}$").WithMessage("Invalid phone number format.");
        }
    }
}
