using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Category.DTOs
{
    public record CategoryDto(Guid Id, string Name, string? Decription);
}
