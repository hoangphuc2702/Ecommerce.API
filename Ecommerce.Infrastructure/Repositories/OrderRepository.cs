//using Ecommerce.Application.Interfaces;
//using Ecommerce.Domain.Entities;
//using Ecommerce.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Ecommerce.Infrastructure.Repositories
//{
//    public class OrderRepository(ApplicationDbContext context) : IOrderRepository
//    {
//        private readonly ApplicationDbContext _context = context;

//        public async Task<Order?> GetByIdAsync(Guid id)
//        {
//            return await _context.Orders
//                .Include(o => o.OrderItems)
//                .FirstOrDefaultAsync(o => o.Id == id);
//        }

//        public void Update(Order order)
//        {
//            _context.Orders.Update(order);
//        }
//    }
//}
