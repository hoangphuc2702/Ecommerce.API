using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Order.Commands
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IZaloPayService _zaloPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CheckoutCommandHandler> _logger;

        public CheckoutCommandHandler(
            IApplicationDbContext context,
            IZaloPayService zaloPayService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CheckoutCommandHandler> logger)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("Please log in to proceed to checkout.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

            if (user == null) throw new NotFoundException("User", userId.Value);

            decimal discountRate = Application.Common.Coupon.RankingPolicy.GetDiscountPercentage(user.Rank);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var orderId = Guid.NewGuid();
                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cart.Items)
                {
                    var product = cartItem.Product;

                    if (product == null || product.IsDeleted)
                        throw new NotFoundException($"Product '{cartItem.ProductId}' is no longer available.");

                    if (product.Stock < cartItem.Quantity)
                        throw new BadRequestException($"Insufficient stock for '{product.Name}'. Available: {product.Stock}.");

                    product.Stock -= cartItem.Quantity;

                    subTotal += cartItem.Quantity * product.Price;

                    orderItems.Add(new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price
                    });
                }

                decimal discountAmount = subTotal * discountRate;
                decimal totalAmount = subTotal - discountAmount;

                var order = new Domain.Entities.Order(orderId)
                {
                    UserId = userId.Value,
                    TotalAmount = Math.Round(totalAmount, 0),
                    SubTotal = subTotal, 
                    DiscountAmount = discountAmount,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = request.ShippingAddress,
                    PhoneNumber = request.PhoneNumber
                };

                await _context.Orders.AddAsync(order, cancellationToken);
                await _context.OrderItems.AddRangeAsync(orderItems, cancellationToken);

                _context.CartItems.RemoveRange(cart.Items);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} placed. SubTotal: {SubTotal}, Discount: {Discount} ({Rate}%), Total: {Total}",
                    orderId, subTotal, discountAmount, discountRate * 100, totalAmount);

                var paymentUrl = await _zaloPayService.CreatePaymentUrl(order);

                return new CheckoutResponse
                {
                    OrderId = orderId,
                    PaymentUrl = paymentUrl
                };
                

            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                _logger.LogWarning(ex, "Concurrency conflict for User {UserId}.", userId);
                throw new BadRequestException("The stock for a product has changed. Please try again.");
            }
            catch (Exception ex)
            {
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                _logger.LogError(ex, "Checkout failed for User {UserId}.", userId);
                throw;
            }
        }
    }
}