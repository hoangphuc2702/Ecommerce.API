using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Features.Cart.DTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(x => x.SubTotal);
        public int TotalItems => Items.Sum(x => x.Quantity);
    }
}
