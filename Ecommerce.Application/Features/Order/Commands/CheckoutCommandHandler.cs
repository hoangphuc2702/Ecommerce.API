using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Features.Order.DTOs;
using Ecommerce.Application.Features.PayOS.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Service.Promotion;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CheckoutCommandHandler> _logger;
        private readonly IShippingService _shippingService;
        private readonly IMediator _mediator;
        private readonly IPromotionEngine _promotionEngine;

        public CheckoutCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CheckoutCommandHandler> logger,
            IShippingService shippingService,
            IMediator mediator,
            IPromotionEngine promotionEngine)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _shippingService = shippingService;
            _mediator = mediator;
            _promotionEngine = promotionEngine;
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
                        UnitPrice = product.Price,
                        Product = product
                    });
                }

                //promotion
                var now = DateTime.UtcNow;
                var allActivePromotions = await _context.PromotionRules
                    .Where(r => r.IsActive && r.StartDate <= now && r.EndDate >= now)
                    .ToListAsync(cancellationToken);

                var promoEvaluation = _promotionEngine.Evaluate(cart.Items, allActivePromotions);
                decimal promoDiscount = promoEvaluation.TotalPromotionDiscount;
                decimal subtotalAfterPromo = subTotal - promoDiscount;

                foreach (var gift in promoEvaluation.Gifts)
                {
                    var giftProduct = await _context.Products.FindAsync(new object[] { gift.ProductId }, cancellationToken);
                    if (giftProduct != null && !giftProduct.IsDeleted)
                    {
                        if (giftProduct.Stock >= gift.Quantity)
                        {
                            giftProduct.Stock -= gift.Quantity;
                            orderItems.Add(new OrderItem
                            {
                                OrderId = orderId,
                                ProductId = gift.ProductId,
                                Quantity = gift.Quantity,
                                UnitPrice = 0,
                                Product = giftProduct
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Not enough stock for gift product {GiftName}", giftProduct.Name);
                        }
                    }
                }

                //Coupon
                decimal couponDiscount = 0;
                if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
                {
                    var coupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.Code.ToLower() == cart.AppliedCouponCode.ToLower(), cancellationToken);

                    if (coupon != null && coupon.CanBeUsed() && subtotalAfterPromo >= coupon.MinOrderValue)
                    {
                        if (coupon.DiscountType == DiscountType.Percentage)
                        {
                            couponDiscount = subtotalAfterPromo * (coupon.Value / 100);
                            if (coupon.MaxDiscountAmount.HasValue && couponDiscount > coupon.MaxDiscountAmount.Value)
                                couponDiscount = coupon.MaxDiscountAmount.Value;
                        }
                        else
                        {
                            couponDiscount = coupon.Value;
                        }

                        if (couponDiscount > subtotalAfterPromo)
                            couponDiscount = subtotalAfterPromo;
                    }
                }

                decimal subtotalAfterCoupon = subtotalAfterPromo - couponDiscount;
                decimal rankDiscountAmount = subtotalAfterCoupon * discountRate;

                decimal totalDiscountAmount = promoDiscount + couponDiscount + rankDiscountAmount;
                var orderItemsForFee = cart.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.Product?.Name ?? "Product",
                    i.Quantity,
                    i.Product?.Price ?? 0,
                    (i.Product?.Price ?? 0) * i.Quantity,
                    (double)(i.Product?.Weight ?? 0.5),
                    (double)(i.Product?.Length ?? 10),
                    (double)(i.Product?.Width ?? 10),
                    (double)(i.Product?.Height ?? 10)
                )).ToList();

                var feeResult = await _shippingService.GetEstimatedFeeAsync(
                    request.ShippingAddress,
                    request.Latitude,
                    request.Longitude,
                    orderItemsForFee,
                    request.ServiceId);

                decimal shippingFee = feeResult.IsSuccess ? feeResult.Data : 0;

                decimal totalAmount = (subTotal - totalDiscountAmount) + shippingFee;

                var order = new Domain.Entities.Order(orderId)
                {
                    UserId = userId.Value,
                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    DiscountAmount = totalDiscountAmount,
                    TotalAmount = Math.Round(totalAmount, 0),
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = request.ShippingAddress,
                    PhoneNumber = request.PhoneNumber,
                    PaymentMethod = request.PaymentMethod,
                    CustomerName = request.CustomerName,
                    ServiceId = request.ServiceId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    OrderItems = orderItems
                };

                await _context.Orders.AddAsync(order, cancellationToken);
                await _context.OrderItems.AddRangeAsync(orderItems, cancellationToken);
                _context.CartItems.RemoveRange(cart.Items);
                cart.AppliedCouponCode = null;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} placed and committed to DB. SubTotal: {SubTotal}, Total: {Total}",
                    orderId, subTotal, totalAmount);

                if (!string.IsNullOrEmpty(request.PaymentMethod) && request.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Processing COD order. Initiating Ahamove shipment creation for OrderId: {OrderId}", orderId);

                    var shipResult = await _shippingService.CreateShipmentAsync(order);

                    if (shipResult.IsSuccess)
                    {
                        var newShipment = new Shipment
                        {
                            OrderId = order.Id,
                            TrackingNumber = shipResult.Data.TrackingNumber,
                            PartnerCode = "AHAMOVE",
                            ServiceId = "SGN-BIKE",
                            Fee = shipResult.Data.Fee,
                            CodAmount = order.TotalAmount,
                            Status = ShipmentStatus.Idle,
                        };
                        order.ShippingFee = shipResult.Data.Fee;
                        order.TotalAmount = (order.SubTotal - order.DiscountAmount) + order.ShippingFee;
                        _context.Orders.Update(order);

                        await _context.Shipments.AddAsync(newShipment, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Ahamove shipment created successfully for COD order. Tracking: {TrackingNumber}", newShipment.TrackingNumber);
                    }
                    else
                    {
                        _logger.LogError("Failed to create Ahamove shipment for COD Order {OrderId}: {Message}", orderId, shipResult.Message);
                    }

                    return new CheckoutResponse
                    {
                        OrderId = orderId,
                        PaymentUrl = string.Empty
                    };
                }
                else
                {
                    _logger.LogInformation("Processing Online Payment. Generating PayOS link for OrderId: {OrderId}", orderId);

                    var paymentUrl = await _mediator.Send(new CreatePayOSLinkCommand(orderId), cancellationToken);

                    return new CheckoutResponse
                    {
                        OrderId = orderId,
                        PaymentUrl = paymentUrl
                    };
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (transaction.GetDbTransaction()?.Connection != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                _logger.LogWarning(ex, "Concurrency conflict detected for User {UserId}.", userId);
                throw new BadRequestException("The stock for a product has changed. Please try again.");
            }
            catch (Exception ex)
            {
                if (transaction.GetDbTransaction()?.Connection != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                _logger.LogError(ex, "Checkout process failed for User {UserId}.", userId);
                throw;
            }
        }
    }
}