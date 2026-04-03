using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.Commands
{
    public class RemoveFromCartCommandHandler : IRequestHandler<DeleteCartCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RemoveFromCartCommandHandler> _logger;

        public RemoveFromCartCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<RemoveFromCartCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task Handle(DeleteCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedException("Please log in to perform this action.");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null)
                throw new NotFoundException("Cart not found.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem == null)
                throw new NotFoundException($"Product with ID {request.ProductId} not found in the cart.");

            cart.Items.Remove(existingItem);
            _logger.LogInformation("User {UserId} removed product {ProductId} from the cart.", userId, request.ProductId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
