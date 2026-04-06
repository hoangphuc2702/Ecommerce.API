using Ecommerce.Application.Features.Auth.Queries;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand : IRequest<string>;

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenCommandHandler(IApplicationDbContext context, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var oldRefreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["REFRESH_TOKEN"];

            if (string.IsNullOrEmpty(oldRefreshToken))
                throw new UnauthorizedException("Your login session has expired, please log in again.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == oldRefreshToken, cancellationToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh Token is invalid or expired.");
            }

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync(cancellationToken);

            _tokenService.WriteTokenToCookie("ACCESS_TOKEN", newAccessToken, DateTime.UtcNow.AddMinutes(15));
            _tokenService.WriteTokenToCookie("REFRESH_TOKEN", newRefreshToken, user.RefreshTokenExpiryTime.Value);

            return "Success";
        }
    }
}
