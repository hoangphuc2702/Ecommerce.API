using Ecommerce.Application.Features.Users.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Users.Queries
{
    public record GetProfileQuery : IRequest<UserProfileDto>;
}
