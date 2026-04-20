using Ecommerce.Application.Interfaces;
using Ecommerce.Core.Entities;
using Ecommerce.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Register
{
    public record RegisterCommand(string Name, string Email, string Password) : IRequest<Guid>;

    public class RegisterCommandHandle : IRequestHandler<RegisterCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandle(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailExists)
            {
                throw new UserAlreadyExistsException(request.Email); ;
            }

            var hashedPassword = _passwordHasher.Hash(request.Password);

            var user = new User(request.Name, request.Email, hashedPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
