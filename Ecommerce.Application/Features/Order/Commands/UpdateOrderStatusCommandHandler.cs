using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Order.Commands
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

        public UpdateOrderStatusCommandHandler(
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderStatusCommandHandler> logger)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                throw new NotFoundException($"Order {request.OrderId} not found.");

            if (order.Status == request.NewStatus)
                return true;

            if (order.Status == OrderStatus.Cancelled)
                throw new BadRequestException("Cannot change the status of an already cancelled order.");

            if (request.NewStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity; 
                    }
                }
                _logger.LogInformation("Stock restored for cancelled order {OrderId} by Admin.", order.Id);
            }

            order.Status = request.NewStatus;

            if (request.NewStatus == OrderStatus.Completed)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == order.UserId, cancellationToken);

                if (user != null)
                {
                    int earnedPoints = Application.Common.Coupon.RankingPolicy.CalculateEarnedPoints(order.TotalAmount);

                    if (earnedPoints > 0)
                    {
                        var oldRank = user.Rank;
                        user.TotalPoints += earnedPoints;

                        user.Rank = Application.Common.Coupon.RankingPolicy.CalculateRank(user.TotalPoints);

                        _logger.LogInformation("User {UserId} earned {Points} points. Total: {Total}. Rank: {Rank}",
                        user.Id, earnedPoints, user.TotalPoints, user.Rank);

                        if (oldRank != user.Rank)
                        {
                            _logger.LogInformation("User {UserId} promoted from {OldRank} to {NewRank}!",
                            user.Id, oldRank, user.Rank);
                        }

                        _context.Users.Update(user);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} status updated to {NewStatus}", order.Id, request.NewStatus);

            return true;
        }
    }
}
