using Ecommerce.Application.Features.Product.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Product.Validators
{
    public class DeleteProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product ID is required for updated.");
        }
    }
}
