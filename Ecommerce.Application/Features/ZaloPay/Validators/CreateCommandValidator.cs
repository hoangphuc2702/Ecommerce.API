using Ecommerce.Application.Features.ZaloPay.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ZaloPay.Validators
{
    public class CreateCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreateCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order code is required.");
        }
    }
}
