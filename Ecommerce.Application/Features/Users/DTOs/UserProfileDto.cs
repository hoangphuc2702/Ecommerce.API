using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Users.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public int TotalPoints { get; set; }
        public required string Rank { get; set; }
    }
}
