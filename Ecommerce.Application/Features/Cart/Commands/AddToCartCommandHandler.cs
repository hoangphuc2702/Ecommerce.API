using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddToCartCommandHandler> _logger;

    public AddToCartCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<AddToCartCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            _logger.LogWarning("AddToCart failed: Unauthorized access.");
            throw new UnauthorizedException("Please log in to add items to cart.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        if (product.Stock < request.Quantity)
        {
            throw new BadRequestException($"Insufficient stock. Available: {product.Stock}");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userId.Value,
                Items = new List<CartItem>()
            };
            _context.Carts.Add(cart);
        }

        var existingItem = cart.Items
            .FirstOrDefault(x => x.ProductId == request.ProductId);

        if (existingItem != null)
        {
            //int updatedQuantity = existingItem.Quantity + request.Quantity;
            //if (updatedQuantity > product.Stock)
            //{
            //    throw new BadRequestException("Total quantity exceeds available stock.");
            //}
            //existingItem.Quantity = updatedQuantity;
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                PriceAtAdd = product.Price
            });
        }

        //var result = 
          await _context.SaveChangesAsync(cancellationToken);

        //if (result <= 0)
        //{
        //    _logger.LogError("Failed to save cart changes for User: {UserId}", userId.Value);
        //}

        return cart.Id;
    }
}