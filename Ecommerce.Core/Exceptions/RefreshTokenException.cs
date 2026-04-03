using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Exceptions
{
    public class RefreshTokenException(string message) : Exception(message);
}
