using Ecommerce.Application.Interfaces;
using Ecommerce.Core.Entities;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Queries
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        public LoginQueryHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }
        public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new LoginFailedException(request.Email);
            }

            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var expiry = DateTime.UtcNow.AddDays(7);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiry;
            await _context.SaveChangesAsync(cancellationToken);

            _tokenService.WriteTokenToCookie("ACCESS_TOKEN", accessToken, DateTime.UtcNow.AddMinutes(15));
            _tokenService.WriteTokenToCookie("REFRESH_TOKEN", refreshToken, expiry);

            return new LoginResponse(
                UserId: user.Id,
                Name: user.Name,
                Role: user.Role ?? "User"
            );
        }
    }
}
