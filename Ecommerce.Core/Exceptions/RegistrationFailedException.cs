using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Exceptions
{
    public class RegistrationFailedException(IEnumerable<string> errorDescriptions) 
        : Exception($"Registration failed with following errors: {string.Join(Environment.NewLine, errorDescriptions)}");
}
