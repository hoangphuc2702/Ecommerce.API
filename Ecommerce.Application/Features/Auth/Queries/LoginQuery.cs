using Ecommerce.Core.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Queries
{
    public record LoginQuery(string Email, string Password) : IRequest<LoginResponse>;
    public record LoginResponse(Guid UserId, string Name, string Role);
}
