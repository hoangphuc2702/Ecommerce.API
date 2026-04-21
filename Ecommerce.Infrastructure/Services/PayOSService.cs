using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Services
{
    public class PayOSService : IPaymentService
    {
        private readonly PayOSClient _payOS;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(PayOSClient payOS, ILogger<PayOSService> logger)
        {
            _payOS = payOS;
            _logger = logger;
        }

        public async Task<Result<string>> CreatePaymentLink(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating PayOS payment link for Order: {OrderCode}", request.OrderCode);

                var paymentData = new CreatePaymentLinkRequest
                {
                    OrderCode = request.OrderCode,
                    Amount = (int)Math.Round(request.Amount, 0),
                    Description = request.Description,
                    CancelUrl = request.CancelUrl,
                    ReturnUrl = request.ReturnUrl,
                };

                var response = await _payOS.PaymentRequests.CreateAsync(paymentData);

                _logger.LogInformation("PayOS link created successfully for Order: {OrderCode}", request.OrderCode);

                return Result<string>.SuccessResult(response.CheckoutUrl, "Create PayOS link successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link for Order: {OrderCode}", request.OrderCode);
                return Result<string>.Failure(new Error("PayOS.CreateError", ex.Message));
            }
        }

        public async Task<Result<PaymentStatusInfo>> GetPaymentStatus(long orderCode)
        {
            try
            {
                var info = await _payOS.PaymentRequests.GetAsync(orderCode);

                var statusInfo = new PaymentStatusInfo(
                    Status: info.Status.ToString(),
                    OrderCode: info.OrderCode,
                    TransactionId: info.Transactions?.FirstOrDefault()?.Reference ?? string.Empty,
                    AmountPaid: info.AmountPaid
                );

                return Result<PaymentStatusInfo>.SuccessResult(statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS status for Order: {OrderCode}", orderCode);
                return Result<PaymentStatusInfo>.Failure(new Error("PayOS.QueryError", ex.Message));
            }
        }

        public async Task<Result<bool>> CancelPaymentLink(long orderCode)
        {
            try
            {
                await _payOS.PaymentRequests.CancelAsync((int)orderCode, "The customer requested cancellation.");
                return Result<bool>.SuccessResult(true, "Cancelled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling PayOS link for Order: {OrderCode}", orderCode);
                return Result<bool>.Failure(new Error("PayOS.CancelError", ex.Message));
            }
        }
    }
}