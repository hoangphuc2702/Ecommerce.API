using Ecommerce.Application.Features.Cart.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;


namespace Ecommerce.Application.Features.Cart.Validators
{
    public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
    {
        public AddToCartCommandValidator()
        {
            RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required and cannot be empty.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(100).WithMessage("You cannot add more than 100 items at once.");
        }
    }
}
