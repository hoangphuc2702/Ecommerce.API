using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string name, object key)
            : base($"Not Found {name} with ID: {key}.")
        {
        }
    }
}
