using OrderDomain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OrderDomain.Repositories
{
    public interface  IOrderRepository
    {
        void Add(Order order);
        void Update(Order order);

        Task<Order> GetByIdAsync(int OrderId);
    }
}
