using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Interfaces
{
    public interface IZaloPayService
    {
        Task<string> CreatePaymentUrl(Order order);

        bool ValidateCallback(string data, string mac);
    }
}
