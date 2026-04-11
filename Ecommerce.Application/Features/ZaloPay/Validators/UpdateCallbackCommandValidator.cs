using Ecommerce.Application.Features.ZaloPay.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ZaloPay.Validators
{
    public class UpdateCallbackCommandValidator : AbstractValidator<UpdateZaloPayCallbackCommand>
    {
        public UpdateCallbackCommandValidator()
        {
            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("ZaloPay data is required.");

            RuleFor(x => x.Mac)
                .NotEmpty().WithMessage("ZaloPay MAC is required.");
        }
    }
}
