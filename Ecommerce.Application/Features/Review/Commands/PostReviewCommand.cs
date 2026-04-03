using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Review.Commands
{
    public record PostReviewCommand(Guid ProductId, int Rating, string Comment) : IRequest<Guid>;
}
