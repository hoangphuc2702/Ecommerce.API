using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Exceptions
{
    public class LoginFailedException(string email) : Exception($"Invalid email: {email} or password.");
}
