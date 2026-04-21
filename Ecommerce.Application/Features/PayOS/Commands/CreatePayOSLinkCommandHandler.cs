using Ecommerce.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.PayOS.Commands
{
    public record CreatePayOSLinkCommand(Guid OrderId) : IRequest<string>;

    public class CreatePayOSLinkCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService) : IRequestHandler<CreatePayOSLinkCommand, string>
    {
        public async Task<string> Handle(CreatePayOSLinkCommand request, CancellationToken ct)
        {
            var order = await unitOfWork.Orders.FindAsync(new object[] { request.OrderId }, ct);
            if (order == null) throw new Exception("Order not found");

            string returnUrl = "https://vacillant-tristful-sallie.ngrok-free.dev/api/v1/payment/payos/checkout/success";
            string cancelUrl = "https://vacillant-tristful-sallie.ngrok-free.dev/api/v1/payment/payos/checkout/cancel";

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var paymentRequest = new PaymentRequest(
                OrderCode: orderCode,
                Amount: order.TotalAmount,
                Description: $"Don hang {order.Id.ToString().Substring(0, 8)}",
                BuyerName: "Customer",
                BuyerEmail: "customer@email.com",
                ReturnUrl: returnUrl,
                CancelUrl: cancelUrl,
                Items: null
            );

            var transaction = new Domain.Entities.Payment
            {
                OrderId = order.Id,
                PaymentProvider = "PayOS",
                ReferenceId = orderCode.ToString(),
                Amount = order.TotalAmount,
                Status = Domain.Enums.PaymentStatus.Pending
            };
            await unitOfWork.Payments.AddAsync(transaction, ct);
            await unitOfWork.SaveChangesAsync(ct);

            var result = await paymentService.CreatePaymentLink(paymentRequest);
            return result.Data;
        }
    }
}
