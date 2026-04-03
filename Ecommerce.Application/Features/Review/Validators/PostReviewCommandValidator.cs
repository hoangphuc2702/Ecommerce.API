using Ecommerce.Application.Features.Review.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.Validators
{
    public class PostReviewCommandValidator : AbstractValidator<PostReviewCommand>
    {
        public PostReviewCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
            RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
        }
    }
}
