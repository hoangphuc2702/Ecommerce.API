using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.ZaloPay.Commands
{
    public record CreatePaymentCommand(Guid OrderId) : IRequest<string>;

    public class CreatePaymentHandler(IApplicationDbContext context, IPaymentService paymentService) : IRequestHandler<CreatePaymentCommand, string>
    {
        public async Task<string> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            var order = await context.Orders.FindAsync([request.OrderId], ct);

            if (order is null)
                throw new NotFoundException(nameof(Order), request.OrderId);

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var paymentRequest = new PaymentRequest(
                OrderCode: orderCode,
                Amount: order.TotalAmount,
                Description: $"Thanh toan don #{orderCode}",
                BuyerName: "Customer",
                BuyerEmail: "customer@email.com",
                ReturnUrl: "https://your-domain.com/payment-success",
                CancelUrl: "https://your-domain.com/payment-cancel",
                Items: null 
            );

            var paymentResult = await paymentService.CreatePaymentLink(paymentRequest);

            if (!paymentResult.IsSuccess)
            {
                throw new BadRequestException("Unable to create payment link");
            }

            return paymentResult.Data ?? string.Empty;
        }
    }
}