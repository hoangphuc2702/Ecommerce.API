using Ecommerce.Application.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<string>> CreatePaymentLink(PaymentRequest request);

    Task<Result<PaymentStatusInfo>> GetPaymentStatus(long orderCode);

    Task<Result<bool>> CancelPaymentLink(long orderCode);
}

public record PaymentItem(string Name, int Quantity, decimal Price);

public record PaymentRequest(
    long OrderCode,
    decimal Amount,
    string Description,
    string BuyerName,
    string BuyerEmail,
    string ReturnUrl,
    string CancelUrl,
    List<PaymentItem>? Items = null
);

public record PaymentStatusInfo(
    string Status,
    long OrderCode,
    string TransactionId,
    decimal AmountPaid);