using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.ZaloPay.Commands
{
    public record CreatePaymentCommand(Guid OrderId) : IRequest<string>;

    public class CreatePaymentHandler(IApplicationDbContext context, IZaloPayService zaloPayService) : IRequestHandler<CreatePaymentCommand, string>
    {
        public async Task<string> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            var order = await context.Orders.FindAsync([request.OrderId], ct);

            if (order is null)
                throw new NotFoundException(nameof(Order), request.OrderId);

            return await zaloPayService.CreatePaymentUrl(order);
        }
    }
}
