using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Auth.Commands
{
    public record LogoutCommand(string refreshToken) : IRequest<string>;

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITokenService _tokenService;

        public LogoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ITokenService tokenService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _tokenService = tokenService;
        }

        public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) throw new UnauthorizedException("User context is missing.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

            if (user == null) throw new NotFoundException("User", userId.Value);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync(cancellationToken);

            _tokenService.DeleteTokenCookie("ACCESS_TOKEN");
            _tokenService.DeleteTokenCookie("REFRESH_TOKEN");
            return "Logout success";
        }
    }
}
