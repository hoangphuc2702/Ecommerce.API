using Ecommerce.Application.Features.Product.Commands;
using FluentValidation.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;


namespace Ecommerce.Application.Features.Product.Validators
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required for update.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }
}
