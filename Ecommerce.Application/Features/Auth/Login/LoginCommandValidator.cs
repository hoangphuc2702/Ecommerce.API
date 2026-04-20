using Ecommerce.Application.Features.Auth.Register;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}
