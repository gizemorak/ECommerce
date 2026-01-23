using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplication.Orders.DTOs;
using OrderApplicationOrders.Commands.CreateOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(int OrderId) : IRequestByServiceResult;
}
