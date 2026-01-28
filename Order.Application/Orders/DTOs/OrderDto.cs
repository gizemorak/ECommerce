using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public OrderStatusDto OrderStatus { get; set; }

        public decimal TotalPrice { get; set; }

        public Guid BuyerId { get; set; }
    }
}
