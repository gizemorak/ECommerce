using Microsoft.EntityFrameworkCore;
using OrderDomain;
using OrderDomain.Orders;
using OrderDomain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderPersistence.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;
        public void Add(Order order) => _dbContext.Set<Order>().Add(order);

        public void Update(Order order)
          => _dbContext.Set<Order>().Update(order);


        public async Task<Order> GetByIdAsync(int OrderId)=> await _dbContext.Set<Order>().FindAsync(OrderId);
      
         
       

    }
}
