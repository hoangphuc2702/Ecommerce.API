using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Exceptions
{
    public class UserAlreadyExistsException(string email) : Exception($"User with email: {email} already exists");
}
