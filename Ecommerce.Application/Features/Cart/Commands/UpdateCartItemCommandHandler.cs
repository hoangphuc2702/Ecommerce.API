using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Cart.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork; 
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;

    public UpdateCartItemCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateCartItemCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
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

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product does not exist or has been deleted.");

        if (product.Stock < request.Quantity)
        {
            throw new BadRequestException($"Insufficient stock. Only {product.Stock} items left in stock.");
        }

        existingItem.Quantity = request.Quantity;
        _logger.LogInformation("User {UserId} updated quantity for product {ProductId} to {Quantity}.", userId, request.ProductId, request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}